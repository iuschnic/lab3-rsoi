using Rentals.Domain.Enums;
using Rentals.Domain.Exceptions;

namespace Rentals.Domain.Models;

public class Rental
{
    public const int UserNameMaxLength = 80;
    public Guid RentalUid { get; private set; }
    public string Username { get; private set; }
    public Guid PaymentUid { get; private set; }
    public Guid CarUid { get; private set; }
    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }
    public RentalStatus Status { get; private set; }

    private Rental(
        Guid rentalUid,
        string userName,
        Guid paymentUid,
        Guid carUid,
        DateTime dateFrom,
        DateTime dateTo,
        RentalStatus status)
    {
        Validate(rentalUid, userName, paymentUid, carUid, dateFrom, dateTo);
        RentalUid = rentalUid;
        Username = userName;
        PaymentUid = paymentUid;
        CarUid = carUid;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Status = status;
    }

    private static void Validate(
        Guid rentalUid,
        string userName,
        Guid paymentUid,
        Guid carUid,
        DateTime dateFrom,
        DateTime dateTo)
    {
        CheckString(userName, "UserName", UserNameMaxLength);
        CheckUid(rentalUid);
        CheckUid(paymentUid);
        CheckUid(carUid);
        CheckDates(dateFrom, dateTo);
    }

    public static Rental Create(
        string userName,
        Guid carUid,
        DateTime dateFrom,
        DateTime dateTo,
        Guid paymentUid)
    {
        return new Rental(Guid.NewGuid(), userName, paymentUid, carUid, dateFrom, dateTo, RentalStatus.InProgress);
    }

    public static Rental Restore(
        Guid rentalUid,
        string userName,
        Guid paymentUid,
        Guid carUid,
        DateTime dateFrom,
        DateTime dateTo,
        RentalStatus status)
    {
        return new Rental(rentalUid, userName, paymentUid, carUid, dateFrom, dateTo, status);
    }

    public void Cancel()
    {
        EnsureInProgress("canceled");
        Status = RentalStatus.Canceled;
    }

    public void Finish()
    {
        EnsureInProgress("finished");
        Status = RentalStatus.Finished;
    }

    private void EnsureInProgress(string action)
    {
        if (Status != RentalStatus.InProgress)
            throw new RentalStatusTransitionException($"Rental {RentalUid} cannot be {action} from status {Status}");
    }

    private static void CheckUid(Guid uuid)
    {
        if (uuid == Guid.Empty)
            throw new DomainValidationException($"Uuid should not be empty");
    }

    private static void CheckString(string value, string valueName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{valueName} should not be empty or whitespace");
        if (value.Length > maxLength)
            throw new DomainValidationException($"{valueName} should not exceed {maxLength} characters");
    }

    private static void CheckDates(DateTime dateFrom, DateTime dateTo)
    {
        if (dateFrom >= dateTo)
            throw new DomainValidationException($"DateFrom should always be before DateTo");
    }
}
