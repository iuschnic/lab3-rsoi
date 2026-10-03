using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Storage.Enums;
using Payments.Storage.Models;

namespace Payments.Storage.Context.Configurations;

public class PaymentDbConfiguration : IEntityTypeConfiguration<PaymentDb>
{
    private const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<PaymentDb> builder)
    {
        builder.ToTable("payment");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(p => p.PaymentUid).HasColumnName("payment_uid").IsRequired();
        builder.HasIndex(p => p.PaymentUid).IsUnique();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(StatusMaxLength)
            .HasConversion(
                status => status.ToString().ToUpperInvariant(),
                value => Enum.Parse<PaymentStatusDb>(value, ignoreCase: true));

        builder.Property(p => p.Price).HasColumnName("price").IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_payment_status",
            "status IN ('PAID', 'CANCELED')"));
    }
}
