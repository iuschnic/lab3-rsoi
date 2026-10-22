using Gateway.Application.Dtos;
using Gateway.Application.Enums;
using Gateway.Application.Exceptions;
using Gateway.Application.InPorts;
using Gateway.Application.Mappers;
using Gateway.Application.Operations;
using Gateway.Application.OutPorts;

namespace Gateway.Application.Services;

public class CarRentalService : ICarRentalService, IDeferredOperationService
{
    private readonly ICarsReadService _carsReadService;
    private readonly ICarsWriteService _carsWriteService;
    private readonly IRentalsReadService _rentalsReadService;
    private readonly IRentalsWriteService _rentalsWriteService;
    private readonly IPaymentsReadService _paymentsReadService;
    private readonly IPaymentsWriteService _paymentsWriteService;
    private readonly IDeferredOperationScheduler _scheduler;

    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    public CarRentalService(ICarsReadService carsReadService, ICarsWriteService carsWriteService, 
        IRentalsReadService rentalsReadService, IRentalsWriteService rentalsWriteService,
        IPaymentsReadService paymentsReadService, IPaymentsWriteService paymentsWriteService,
        IDeferredOperationScheduler scheduler)
    {
        _carsReadService = carsReadService;
        _carsWriteService = carsWriteService;
        _rentalsReadService = rentalsReadService;
        _rentalsWriteService = rentalsWriteService;
        _paymentsReadService = paymentsReadService;
        _paymentsWriteService = paymentsWriteService;
        _scheduler = scheduler;
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
        var remaining = await CancelRentalUntilBlockedAsync(new CancelOpGetRental(rentalUid, username), 
            cancellationToken);

        if (remaining is not null)
            await _scheduler.ScheduleAsync(remaining, DateTimeOffset.UtcNow + RetryDelay, cancellationToken);
    }

    public async Task FinishRentalAsync(
        Guid rentalUid,
        string username,
        CancellationToken cancellationToken = default)
    {
        var remaining = await FinishRentalUntilBlockedAsync(new FinishOpGetRental(rentalUid, username),
            cancellationToken);

        if (remaining is not null)
            await _scheduler.ScheduleAsync(remaining, DateTimeOffset.UtcNow + RetryDelay, cancellationToken);
    }

    public async Task<DeferredOperation?> ProcessAsync(
        DeferredOperation operation,
        CancellationToken cancellationToken = default)
    {
        return operation switch
        {
            CancelRentalDeferredOperation cancel =>
                await CancelRentalUntilBlockedAsync(cancel, cancellationToken),
            FinishRentalDeferredOperation finish =>
                await FinishRentalUntilBlockedAsync(finish, cancellationToken),
            _ => throw new NotSupportedException($"Unsupported operation: {operation.GetType().Name}")
        };
    }

    private async Task<CancelRentalDeferredOperation?> CancelRentalUntilBlockedAsync(
        CancelRentalDeferredOperation operation,
        CancellationToken cancellationToken)
    {
        CancelRentalDeferredOperation? current = operation;

        while (current is not null)
        {
            try
            {
                current = await CancelRentalExecuteStepAsync(current, cancellationToken);
            }
            catch (ServiceUnavailableException)
            {
                return current;
            }
        }

        return null;
    }

    private async Task<FinishRentalDeferredOperation?> FinishRentalUntilBlockedAsync(
        FinishRentalDeferredOperation operation,
        CancellationToken cancellationToken)
    {
        FinishRentalDeferredOperation? current = operation;

        while (current is not null)
        {
            try
            {
                current = await FinishRentalExecuteStepAsync(current, cancellationToken);
            }
            catch (ServiceUnavailableException)
            {
                return current;
            }
        }

        return null;
    }

    private async Task<CancelRentalDeferredOperation?> CancelRentalExecuteStepAsync(
        CancelRentalDeferredOperation operation,
        CancellationToken cancellationToken)
    {
        switch (operation)
        {
            case CancelOpGetRental get:
            {
                var rental = await _rentalsReadService.GetRentalAsync(
                    get.RentalUid,
                    get.Username,
                    cancellationToken)
                    ?? throw new AppNotFoundException(
                        $"Rental {get.RentalUid} was not found");

                if (rental.Status != RentalStatusDto.InProgress)
                    throw new AppConflictException($"Rental {get.RentalUid} is not in progress and cannot be canceled");

                return new CancelOpReleaseCar(
                    rental.RentalUid,
                    get.Username,
                    rental.CarUid,
                    rental.PaymentUid);
            }

            case CancelOpReleaseCar release:
            {
                await _carsWriteService.ReleaseCarAsync(
                    release.CarUid, cancellationToken);

                return new CancelOpCancelRental(
                    release.RentalUid,
                    release.Username,
                    release.PaymentUid);
            }

            case CancelOpCancelRental cancel:
            {
                // TODO CancelRental не должен возвращать модель
                await _rentalsWriteService.CancelRentalAsync(
                    cancel.RentalUid,
                    cancel.Username,
                    cancellationToken);

                return new CancelOpCancelPayment(cancel.PaymentUid);
            }

            case CancelOpCancelPayment payment:
            {
                await _paymentsWriteService.CancelPaymentAsync(
                    payment.PaymentUid, cancellationToken);

                return null;
            }

            default:
                throw new NotSupportedException(
                    $"Unsupported operation: {operation.GetType().Name}");
        }
    }

    private async Task<FinishRentalDeferredOperation?> FinishRentalExecuteStepAsync(
        FinishRentalDeferredOperation operation,
        CancellationToken cancellationToken)
    {
        switch (operation)
        {
            case FinishOpGetRental get:
            {
                var rental = await _rentalsReadService.GetRentalAsync(
                    get.RentalUid,
                    get.Username,
                    cancellationToken)
                    ?? throw new AppNotFoundException(
                        $"Rental {get.RentalUid} was not found");

                if (rental.Status != RentalStatusDto.InProgress)
                    throw new AppConflictException($"Rental {get.RentalUid} is not in progress and cannot be canceled");

                return new FinishOpFinishRental(
                    rental.RentalUid,
                    get.Username,
                    rental.CarUid);
            }

            case FinishOpFinishRental finish:
            {
                // TODO FinishRental не должен возвращать модель
                await _rentalsWriteService.FinishRentalAsync(
                    finish.RentalUid,
                    finish.Username,
                    cancellationToken);

                return new FinishOpReleaseCar(
                    finish.CarUid);
            }

            case FinishOpReleaseCar release:
            {
                await _carsWriteService.ReleaseCarAsync(
                    release.CarUid,
                    cancellationToken);

                return null;
            }

            default:
                throw new NotSupportedException(
                    $"Unsupported operation: {operation.GetType().Name}");
        }
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
