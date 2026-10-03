using Gateway.Application.Dtos;
using Gateway.Application.Exceptions;
using Gateway.Application.InPorts;
using Gateway.Application.Mappers;
using Gateway.Application.OutPorts;

namespace Gateway.Application.Services;

public class CarRentalService : ICarRentalService
{
    private readonly ICarsService _carsService;
    private readonly IRentalsService _rentalsService;
    private readonly IPaymentsService _paymentService;

    public CarRentalService(ICarsService carsService, IRentalsService rentalService,
        IPaymentsService paymentsService)
    {
        _carsService = carsService;
        _rentalsService = rentalService;
        _paymentService = paymentsService;
    }

    public async Task<PagedResultDto<CarDto>> GetCarsAsync(
        int page,
        int size,
        bool showAll,
        CancellationToken cancellationToken = default)
    {
        return await _carsService.GetCarsAsync(page, size, showAll, cancellationToken);
    }

    public async Task<IReadOnlyList<RentalAggregate>> GetUserRentalsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalsService.GetRentalsAsync(username, cancellationToken);
        if (rentals.Count == 0)
            return Array.Empty<RentalAggregate>();

        var cars = await _carsService.GetCarsAsync(
            rentals.Select(r => r.CarUid).Distinct().ToList(), cancellationToken);
        var payments = await _paymentService.GetPaymentsAsync(
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
        var rental = await _rentalsService.GetRentalAsync(rentalUid, username, cancellationToken)
            ?? throw new AppNotFoundException($"Rental with uid {rentalUid} was not found");
        var car = await _carsService.GetCarAsync(rental.CarUid, cancellationToken)
            ?? throw new AppInconsistentDataException($"Car {rental.CarUid} referenced by rental {rentalUid} was not found");
        var payment = await _paymentService.GetPaymentAsync(rental.PaymentUid, cancellationToken)
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

        var car = await _carsService.GetCarAsync(carUid, cancellationToken)
            ?? throw new AppNotFoundException($"Car with uid {carUid} was not found");

        await _carsService.ReserveCarAsync(carUid, cancellationToken);

        PaymentDto payment;
        try
        {
            payment = await _paymentService.CreatePaymentAsync(days * car.Price, cancellationToken);
        }
        catch
        {
            await ReleaseCarSafeAsync(carUid, cancellationToken);
            throw;
        }

        RentalDto rental;
        try
        {
            rental = await _rentalsService.CreateRentalAsync(
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
        var rental = await _rentalsService.CancelRentalAsync(rentalUid, username, cancellationToken);

        await _carsService.ReleaseCarAsync(rental.CarUid, cancellationToken);
        await _paymentService.CancelPaymentAsync(rental.PaymentUid, cancellationToken);
    }

    public async Task FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var rental = await _rentalsService.FinishRentalAsync(rentalUid, username, cancellationToken);

        await _carsService.ReleaseCarAsync(rental.CarUid, cancellationToken);
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
            await _carsService.ReleaseCarAsync(carUid, cancellationToken);
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
            await _paymentService.CancelPaymentAsync(paymentUid, cancellationToken);
        }
        catch (AppException)
        {
            // Компенсация не удалась: исходная ошибка важнее, платеж останется в статусе PAID.
        }
    }
}
