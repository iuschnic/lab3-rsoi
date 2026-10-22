namespace Gateway.Application.Operations;

public abstract record CancelRentalDeferredOperation : DeferredOperation;

public record CancelOpGetRental(
    Guid RentalUid,
    string Username) : CancelRentalDeferredOperation;

public record CancelOpReleaseCar(
    Guid RentalUid,
    string Username,
    Guid CarUid,
    Guid PaymentUid) : CancelRentalDeferredOperation;

public record CancelOpCancelRental(
    Guid RentalUid,
    string Username,
    Guid PaymentUid) : CancelRentalDeferredOperation;

public record CancelOpCancelPayment(
    Guid PaymentUid) : CancelRentalDeferredOperation;