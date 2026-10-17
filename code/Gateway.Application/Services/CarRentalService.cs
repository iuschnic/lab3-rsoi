using Gateway.Application.Dtos;
using Gateway.Application.Exceptions;
using Gateway.Application.InPorts;
using Gateway.Application.Mappers;
using Gateway.Application.OutPorts;

namespace Gateway.Application.Services;

public class CarRentalService : ICarRentalService
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
        var rentals = await _rentalsReadService.GetRentalsAsync(username, cancellationToken);
        if (rentals.Count == 0)
            return Array.Empty<RentalAggregate>();

        var cars = await _carsReadService.GetCarsAsync(
            rentals.Select(r => r.CarUid).Distinct().ToList(), cancellationToken);
        var payments = await _paymentsReadService.GetPaymentsAsync(
            rentals.Select(r => r.PaymentUid).Distinct().ToList(), cancellationToken);

        var carsByUid = cars.ToDictionary(c => c.CarUid);
        var paymentsByUid = payments.ToDictionary(p => p.PaymentUid);

        return rentals
            .Select(r => r.ToAggregate(
                GetCarOrThrow(carsByUid, r),
                GetPaymentOrThrow(paymentsByUid, r)))
            .ToList();
    }

    public async Task<RentalAggregate> GetUserRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsReadService.GetRentalAsync(rentalUid, username, cancellationToken)
            ?? throw new AppNotFoundException($"Rental with uid {rentalUid} was not found");
        var car = await _carsReadService.GetCarAsync(rental.CarUid, cancellationToken)
            ?? throw new AppInconsistentDataException($"Car {rental.CarUid} referenced by rental {rentalUid} was not found");
        var payment = await _paymentsReadService.GetPaymentAsync(rental.PaymentUid, cancellationToken)
            ?? throw new AppInconsistentDataException($"Payment {rental.PaymentUid} referenced by rental {rentalUid} was not found");

        return rental.ToAggregate(car, payment);
    }

    public async Task<RentalAggregate> CreateRentalAsync(
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

    public async Task CancelRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsWriteService.CancelRentalAsync(rentalUid, username, cancellationToken);

        await _carsWriteService.ReleaseCarAsync(rental.CarUid, cancellationToken);
        await _paymentsWriteService.CancelPaymentAsync(rental.PaymentUid, cancellationToken);
    }

    public async Task FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsWriteService.FinishRentalAsync(rentalUid, username, cancellationToken);

        await _carsWriteService.ReleaseCarAsync(rental.CarUid, cancellationToken);
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
