using System.Text.Json;
using Gateway.Api.Responses;
using Xunit;

namespace UnitTestsGateway;

public class RentalResponseSerializationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void MissingPaymentIsAnEmptyObjectInRentalResponse()
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new RentalResponse(), Options));

        var payment = json.RootElement.GetProperty("payment");
        Assert.Equal(JsonValueKind.Object, payment.ValueKind);
        Assert.Empty(payment.EnumerateObject());
    }

    [Fact]
    public void PresentPaymentRetainsItsFieldsAndWebNamingPolicy()
    {
        var paymentUid = Guid.NewGuid();
        var response = new RentalResponse
        {
            Payment = new PaymentInfo { PaymentUid = paymentUid, Status = "CANCELED", Price = 10500 }
        };

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response, Options));

        var payment = json.RootElement.GetProperty("payment");
        Assert.Equal(paymentUid, payment.GetProperty("paymentUid").GetGuid());
        Assert.Equal("CANCELED", payment.GetProperty("status").GetString());
        Assert.Equal(10500, payment.GetProperty("price").GetInt32());
        Assert.Equal(3, payment.EnumerateObject().Count());
    }
}
