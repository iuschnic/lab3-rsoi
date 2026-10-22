namespace Gateway.Application.Operations;

public abstract record FinishRentalDeferredOperation;

public record FinishOpGetRental(
    Guid RentalUid,
    string Username) : FinishRentalDeferredOperation;

public record FinishOpFinishRental(
    Guid RentalUid,
    string Username,
    Guid CarUid) : FinishRentalDeferredOperation;

public record FinishOpReleaseCar(
    Guid CarUid) : FinishRentalDeferredOperation;