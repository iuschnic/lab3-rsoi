using Gateway.Application.Dtos;
using Gateway.Application.Exceptions;
using Gateway.Application.InPorts;
using Gateway.Application.Mappers;
using Gateway.Application.Operations;
using Gateway.Application.OutPorts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gateway.Application.Services;

public class CarRentalService : ICarRentalService, IDeferredOperationService
{
    private readonly ICarsReadService _carsReadService;
    private readonly ICarsWriteService _carsWriteService;
    private readonly IRentalsReadService _rentalsReadService;
    private readonly IRentalsWriteService _rentalsWriteService;
    private readonly IPaymentsReadService _paymentsReadService;
    private readonly IPaymentsWriteService _paymentsWriteService;

    public CarRentalService(ICarsReadService carsReadService, ICarsWriteService carsWriteService, 
        IRentalsReadService rentalsReadService, IRentalsWriteService rentalsWriteService,
        IPaymentsReadService paymentsReadService, IPaymentsWriteService paymentsWriteService)
    {
        _carsReadService = carsReadService;
        _carsWriteService = carsWriteService;
        _rentalsReadService = rentalsReadService;
        _rentalsWriteService = rentalsWriteService;
        _paymentsReadService = paymentsReadService;
        _paymentsWriteService = paymentsWriteService;
    }

    public Task ProcessAsync(
    DeferredOperation operation,
    CancellationToken cancellationToken = default)
    {
        return operation switch
        {
            CreateRentalOperation create =>
                ExecuteCreateRentalAsync(create, cancellationToken),

            CancelRentalOperation cancel =>
                ExecuteCancelRentalAsync(cancel, cancellationToken),

            FinishRentalOperation finish =>
                ExecuteFinishRentalAsync(finish, cancellationToken),

            _ => throw new NotSupportedException(
                $"Unsupported operation type: {operation.GetType().Name}")
        };
    }

    public async Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default)
    {
        return await _carsReadService.GetCarsAsync(page, size, showAll, cancellationToken);
    }

    public async Task<IReadOnlyList<RentalAggregate>> GetUserRentalsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalsReadService.GetRentalsAsync(
            username, cancellationToken);

        if (rentals.Count == 0)
            return [];

        IReadOnlyList<CarDto>? cars;
        try
        {
            cars = await _carsReadService.GetCarsAsync(
                rentals.Select(r => r.CarUid).Distinct().ToList(),
                cancellationToken);
        }
        catch (ServiceUnavailableException)
        {
            cars = null;
        }

        IReadOnlyList<PaymentDto>? payments;
        try
        {
            payments = await _paymentsReadService.GetPaymentsAsync(
                rentals.Select(r => r.PaymentUid).Distinct().ToList(),
                cancellationToken);
        }
        catch (ServiceUnavailableException)
        {
            payments = null;
        }

        var carsByUid = cars?.ToDictionary(c => c.CarUid);
        var paymentsByUid = payments?.ToDictionary(p => p.PaymentUid);

        return rentals
            .Select(r => r.ToAggregate(
                carsByUid is null ? null : GetCarOrThrow(carsByUid, r),
                paymentsByUid is null ? null : GetPaymentOrThrow(paymentsByUid, r)))
            .ToList();
    }

    public async Task<RentalAggregate> GetUserRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsReadService.GetRentalAsync(rentalUid, username, cancellationToken)
            ?? throw new AppNotFoundException($"Rental with uid {rentalUid} was not found");

        CarDto? car;
        try
        {
            car = await _carsReadService.GetCarAsync(rental.CarUid, cancellationToken)
                ?? throw new AppInconsistentDataException($"Car {rental.CarUid} referenced by rental {rentalUid} was not found");
        }
        catch (ServiceUnavailableException)
        {
            car = null;
        }

        PaymentDto? payment;
        try
        {
            payment = await _paymentsReadService.GetPaymentAsync(rental.PaymentUid, cancellationToken)
            ?? throw new AppInconsistentDataException($"Payment {rental.PaymentUid} referenced by rental {rentalUid} was not found");
        }
        catch (ServiceUnavailableException)
        {
            payment = null;
        }
        return rental.ToAggregate(car, payment);
    }

    public async Task<RentalAggregate> CreateRentalAsync(
        Guid carUid,
        string username,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default)
    {
        return await CreateRentalCoreAsync(carUid, username, dateFrom, dateTo, cancellationToken);
    }

    public async Task CancelRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        await CancelRentalCoreAsync(rentalUid, username, cancellationToken);
    }

    public async Task FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        await FinishRentalCoreAsync(rentalUid, username, cancellationToken);
    }

    private async Task ExecuteCreateRentalAsync(CreateRentalOperation create,  CancellationToken cancellationToken)
    {
        await CreateRentalCoreAsync(create.CarUid, create.Username, create.DateFrom, create.DateTo, cancellationToken);
    }

    private async Task ExecuteCancelRentalAsync(CancelRentalOperation cancel, CancellationToken cancellationToken)
    {
        await CancelRentalCoreAsync(cancel.RentalUid, cancel.Username, cancellationToken);
    }

    private async Task ExecuteFinishRentalAsync(FinishRentalOperation finish, CancellationToken cancellationToken)
    {
        await FinishRentalCoreAsync(finish.RentalUid, finish.Username, cancellationToken);
    }

    private async Task<RentalAggregate> CreateRentalCoreAsync(
        Guid carUid,
        string username,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default)
    {
        var days = dateTo.DayNumber - dateFrom.DayNumber;
        if (days <= 0)
            throw new AppValidationException("dateTo", "DateTo should be after dateFrom");

        var car = await _carsReadService.GetCarAsync(carUid, cancellationToken)
            ?? throw new AppNotFoundException($"Car with uid {carUid} was not found");

        await _carsWriteService.ReserveCarAsync(carUid, cancellationToken);

        PaymentDto payment;
        try
        {
            payment = await _paymentsWriteService.CreatePaymentAsync(days * car.Price, cancellationToken);
        }
        catch
        {
            await ReleaseCarSafeAsync(carUid, cancellationToken);
            throw;
        }

        RentalDto rental;
        try
        {
            rental = await _rentalsWriteService.CreateRentalAsync(
                new CreateRentalCommand
                {
                    Username = username,
                    CarUid = carUid,
                    PaymentUid = payment.PaymentUid,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                },
                cancellationToken);
        }
        catch
        {
            await CancelPaymentSafeAsync(payment.PaymentUid, cancellationToken);
            await ReleaseCarSafeAsync(carUid, cancellationToken);
            throw;
        }

        return rental.ToAggregate(car, payment);
    }

    private async Task FinishRentalCoreAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsWriteService.FinishRentalAsync(rentalUid, username, cancellationToken);

        await _carsWriteService.ReleaseCarAsync(rental.CarUid, cancellationToken);
    }

    private async Task CancelRentalCoreAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsWriteService.CancelRentalAsync(rentalUid, username, cancellationToken);

        await _carsWriteService.ReleaseCarAsync(rental.CarUid, cancellationToken);
        await _paymentsWriteService.CancelPaymentAsync(rental.PaymentUid, cancellationToken);
    }

    private static CarDto GetCarOrThrow(IReadOnlyDictionary<Guid, CarDto> carsByUid, RentalDto rental)
    {
        if (!carsByUid.TryGetValue(rental.CarUid, out var car))
            throw new AppInconsistentDataException(
                $"Car {rental.CarUid} referenced by rental {rental.RentalUid} was not found");

        return car;
    }

    private static PaymentDto GetPaymentOrThrow(IReadOnlyDictionary<Guid, PaymentDto> paymentsByUid, RentalDto rental)
    {
        if (!paymentsByUid.TryGetValue(rental.PaymentUid, out var payment))
            throw new AppInconsistentDataException(
                $"Payment {rental.PaymentUid} referenced by rental {rental.RentalUid} was not found");

        return payment;
    }

    private async Task ReleaseCarSafeAsync(Guid carUid, CancellationToken cancellationToken)
    {
        try
        {
            await _carsWriteService.ReleaseCarAsync(carUid, cancellationToken);
        }
        catch (AppException)
        {
            // Компенсация не удалась: исходная ошибка важнее, машина останется в резерве.
        }
    }

    private async Task CancelPaymentSafeAsync(Guid paymentUid, CancellationToken cancellationToken)
    {
        try
        {
            await _paymentsWriteService.CancelPaymentAsync(paymentUid, cancellationToken);
        }
        catch (AppException)
        {
            // Компенсация не удалась: исходная ошибка важнее, платеж останется в статусе PAID.
        }
    }
}
