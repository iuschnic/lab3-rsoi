namespace Gateway.Application.Operations;

public abstract record DeferredOperation;

public record CreateRentalOperation(
    Guid CarUid,
    string Username,
    DateOnly DateFrom,
    DateOnly DateTo) : DeferredOperation;

public record CancelRentalOperation(
    Guid RentalUid,
    string Username) : DeferredOperation;

public record FinishRentalOperation(
    Guid RentalUid,
    string Username) : DeferredOperation;