using Rentals.Storage.Enums;

namespace Rentals.Storage.Models;

public class RentalDb
{
    public int Id { get; set; }
    public Guid RentalUid { get; set; }
    public string Username { get; set; } = string.Empty;
    public Guid PaymentUid { get; set; }
    public Guid CarUid { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public RentalStatusDb Status { get; set; }
}
