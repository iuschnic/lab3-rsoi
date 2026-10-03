using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentals.Storage.Enums;
using Rentals.Storage.Models;
using Rentals.Domain.Models;

namespace Rentals.Storage.Context.Configurations;

public class RentalDbConfiguration : IEntityTypeConfiguration<RentalDb>
{
    private const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<RentalDb> builder)
    {
        builder.ToTable("rental");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(r => r.RentalUid).HasColumnName("rental_uid").IsRequired();
        builder.HasIndex(r => r.RentalUid).IsUnique();

        builder.Property(r => r.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(Rental.UserNameMaxLength);
        builder.HasIndex(r => r.Username);

        builder.Property(r => r.PaymentUid).HasColumnName("payment_uid").IsRequired();
        builder.Property(r => r.CarUid).HasColumnName("car_uid").IsRequired();

        builder.Property(r => r.DateFrom)
            .HasColumnName("date_from")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(r => r.DateTo)
            .HasColumnName("date_to")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(StatusMaxLength)
            .HasConversion(
                status => ToDbString(status),
                value => FromDbString(value));

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_rental_status",
            "status IN ('IN_PROGRESS', 'FINISHED', 'CANCELED')"));
    }

    private static string ToDbString(RentalStatusDb status) => status switch
    {
        RentalStatusDb.InProgress => "IN_PROGRESS",
        RentalStatusDb.Finished => "FINISHED",
        RentalStatusDb.Canceled => "CANCELED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown rental status"),
    };

    private static RentalStatusDb FromDbString(string value) => value switch
    {
        "IN_PROGRESS" => RentalStatusDb.InProgress,
        "FINISHED" => RentalStatusDb.Finished,
        "CANCELED" => RentalStatusDb.Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown rental status"),
    };
}
