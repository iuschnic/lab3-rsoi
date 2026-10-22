using FluentAssertions;
using Rentals.Domain.Enums;
using Rentals.Domain.Exceptions;
using Xunit;
using Rental = Rentals.Domain.Models.Rental;

namespace UnitTestsRentals;

public class RentalTests
{
    private const string ValidUserName = "Test Max";
    private static readonly Guid ValidCarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab");
    private static readonly Guid ValidPaymentUid = Guid.Parse("238c733c-fb1e-40a9-aadb-73cb8f90675d");
    private static readonly DateTime ValidDateFrom = new(2021, 10, 8, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ValidDateTo = new(2021, 10, 11, 0, 0, 0, DateTimeKind.Utc);

    private static Rental CreateValidRental()
        => Rental.Create(ValidUserName, ValidCarUid, ValidDateFrom, ValidDateTo, ValidPaymentUid);

    // успешное создание

    [Fact]
    public void Create_WithValidData_SetsAllPropertiesAndInProgressStatus()
    {
        var rental = CreateValidRental();

        rental.RentalUid.Should().NotBeEmpty();
        rental.Username.Should().Be(ValidUserName);
        rental.CarUid.Should().Be(ValidCarUid);
        rental.PaymentUid.Should().Be(ValidPaymentUid);
        rental.DateFrom.Should().Be(ValidDateFrom);
        rental.DateTo.Should().Be(ValidDateTo);
        rental.Status.Should().Be(RentalStatus.InProgress);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDifferentUids()
    {
        var first = CreateValidRental();
        var second = CreateValidRental();

        first.RentalUid.Should().NotBe(second.RentalUid);
    }

    // валидация имени пользователя

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingOrWhitespaceUserName_Throws(string? invalidUserName)
    {
        var act = () => Rental.Create(invalidUserName!, ValidCarUid, ValidDateFrom, ValidDateTo, ValidPaymentUid);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*UserName*");
    }

    [Fact]
    public void Create_WithUserNameAtMaxLength_IsAllowed()
    {
        var maxUserName = new string('a', Rental.UserNameMaxLength);

        var rental = Rental.Create(maxUserName, ValidCarUid, ValidDateFrom, ValidDateTo, ValidPaymentUid);

        rental.Username.Should().Be(maxUserName);
    }

    [Fact]
    public void Create_WithUserNameExceedingMaxLength_Throws()
    {
        var tooLongUserName = new string('a', Rental.UserNameMaxLength + 1);

        var act = () => Rental.Create(tooLongUserName, ValidCarUid, ValidDateFrom, ValidDateTo, ValidPaymentUid);

        act.Should().Throw<DomainValidationException>()
            .WithMessage($"*{Rental.UserNameMaxLength}*");
    }

    // валидация uid

    [Fact]
    public void Create_WithEmptyCarUid_Throws()
    {
        var act = () => Rental.Create(ValidUserName, Guid.Empty, ValidDateFrom, ValidDateTo, ValidPaymentUid);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Uuid*");
    }

    [Fact]
    public void Create_WithEmptyPaymentUid_Throws()
    {
        var act = () => Rental.Create(ValidUserName, ValidCarUid, ValidDateFrom, ValidDateTo, Guid.Empty);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Uuid*");
    }

    // валидация дат

    [Fact]
    public void Create_WithDateFromAfterDateTo_Throws()
    {
        var act = () => Rental.Create(ValidUserName, ValidCarUid, ValidDateTo, ValidDateFrom, ValidPaymentUid);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*DateFrom*");
    }

    [Fact]
    public void Create_WithEqualDates_Throws()
    {
        var act = () => Rental.Create(ValidUserName, ValidCarUid, ValidDateFrom, ValidDateFrom, ValidPaymentUid);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*DateFrom*");
    }

    // восстановление из базы

    [Fact]
    public void Restore_SetsProvidedFields()
    {
        var rentalUid = Guid.NewGuid();

        var rental = Rental.Restore(rentalUid, ValidUserName, ValidPaymentUid, ValidCarUid, ValidDateFrom,
            ValidDateTo, RentalStatus.Finished);

        rental.RentalUid.Should().Be(rentalUid);
        rental.Status.Should().Be(RentalStatus.Finished);
    }

    [Fact]
    public void Restore_WithEmptyRentalUid_Throws()
    {
        var act = () => Rental.Restore(Guid.Empty, ValidUserName, ValidPaymentUid, ValidCarUid, ValidDateFrom,
            ValidDateTo, RentalStatus.InProgress);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Uuid*");
    }

    // переходы статусов

    [Fact]
    public void Finish_OnInProgressRental_SetsFinishedStatus()
    {
        var rental = CreateValidRental();

        rental.Finish();

        rental.Status.Should().Be(RentalStatus.Finished);
    }

    [Fact]
    public void Cancel_OnInProgressRental_SetsCanceledStatus()
    {
        var rental = CreateValidRental();

        rental.Cancel();

        rental.Status.Should().Be(RentalStatus.Canceled);
    }

    [Fact]
    public void Finish_CalledTwice_Throws()
    {
        var rental = CreateValidRental();
        rental.Finish();

        var act = () => rental.Finish();

        act.Should().Throw<RentalStatusTransitionException>()
            .WithMessage($"*{rental.RentalUid}*");
    }

    [Fact]
    public void Cancel_OnFinishedRental_ThrowsAndKeepsStatus()
    {
        var rental = CreateValidRental();
        rental.Finish();

        var act = () => rental.Cancel();

        act.Should().Throw<RentalStatusTransitionException>();
        rental.Status.Should().Be(RentalStatus.Finished);
    }

    [Fact]
    public void Finish_OnCanceledRental_ThrowsAndKeepsStatus()
    {
        var rental = CreateValidRental();
        rental.Cancel();

        var act = () => rental.Finish();

        act.Should().Throw<RentalStatusTransitionException>();
        rental.Status.Should().Be(RentalStatus.Canceled);
    }
}
