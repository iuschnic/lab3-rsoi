using FluentAssertions;
using Payments.Domain.Enums;
using Payments.Domain.Exceptions;
using Xunit;
using Payment = Payments.Domain.Models.Payment;

namespace UnitTestsPayments;

public class PaymentTests
{
    private const int ValidPrice = 10500;

    // успешное создание

    [Fact]
    public void Create_WithValidPrice_SetsPaidStatusAndGeneratesUid()
    {
        var payment = Payment.Create(ValidPrice);

        payment.PaymentUid.Should().NotBeEmpty();
        payment.Status.Should().Be(PaymentStatus.Paid);
        payment.Price.Should().Be(ValidPrice);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDifferentUids()
    {
        var first = Payment.Create(ValidPrice);
        var second = Payment.Create(ValidPrice);

        first.PaymentUid.Should().NotBe(second.PaymentUid);
    }

    // валидация суммы

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10500)]
    public void Create_WithNonPositivePrice_Throws(int invalidPrice)
    {
        var act = () => Payment.Create(invalidPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Price*");
    }

    [Fact]
    public void Create_WithMinimalPositivePrice_IsAllowed()
    {
        var payment = Payment.Create(1);

        payment.Price.Should().Be(1);
    }

    // восстановление из базы

    [Fact]
    public void Restore_SetsProvidedFields()
    {
        var paymentUid = Guid.NewGuid();

        var payment = Payment.Restore(paymentUid, PaymentStatus.Canceled, ValidPrice);

        payment.PaymentUid.Should().Be(paymentUid);
        payment.Status.Should().Be(PaymentStatus.Canceled);
        payment.Price.Should().Be(ValidPrice);
    }

    [Fact]
    public void Restore_WithEmptyUid_Throws()
    {
        var act = () => Payment.Restore(Guid.Empty, PaymentStatus.Paid, ValidPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Uuid*");
    }

    // отмена платежа

    [Fact]
    public void Cancel_OnPaidPayment_SetsCanceledStatus()
    {
        var payment = Payment.Create(ValidPrice);

        payment.Cancel();

        payment.Status.Should().Be(PaymentStatus.Canceled);
    }

    [Fact]
    public void Cancel_DoesNotChangePrice()
    {
        var payment = Payment.Create(ValidPrice);

        payment.Cancel();

        payment.Price.Should().Be(ValidPrice);
    }

    [Fact]
    public void Cancel_CalledTwice_Throws()
    {
        var payment = Payment.Create(ValidPrice);
        payment.Cancel();

        var act = () => payment.Cancel();

        act.Should().Throw<PaymentStatusTransitionException>()
            .WithMessage($"*{payment.PaymentUid}*");
    }

    [Fact]
    public void Cancel_OnCanceledRestoredPayment_ThrowsAndKeepsStatus()
    {
        var payment = Payment.Restore(Guid.NewGuid(), PaymentStatus.Canceled, ValidPrice);

        var act = () => payment.Cancel();

        act.Should().Throw<PaymentStatusTransitionException>();
        payment.Status.Should().Be(PaymentStatus.Canceled);
    }
}
