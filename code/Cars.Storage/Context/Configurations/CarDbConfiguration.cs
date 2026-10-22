using Cars.Storage.Enums;
using Cars.Domain.Models;
using Cars.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cars.Storage.Context.Configurations;

public class CarDbConfiguration : IEntityTypeConfiguration<CarDb>
{
    private const int TypeMaxLength = 20;

    public void Configure(EntityTypeBuilder<CarDb> builder)
    {
        builder.ToTable("cars");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(c => c.CarUid).HasColumnName("car_uid").IsRequired();
        builder.HasIndex(c => c.CarUid).IsUnique();

        builder.Property(c => c.Brand)
            .HasColumnName("brand")
            .IsRequired()
            .HasMaxLength(Car.BrandMaxLength);

        builder.Property(c => c.Model)
            .HasColumnName("model")
            .IsRequired()
            .HasMaxLength(Car.ModelMaxLength);

        builder.Property(c => c.RegistrationNumber)
            .HasColumnName("registration_number")
            .IsRequired()
            .HasMaxLength(Car.RegistrationNumberMaxLength);

        builder.Property(c => c.Power).HasColumnName("power").IsRequired();
        builder.Property(c => c.Price).HasColumnName("price").IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasMaxLength(TypeMaxLength)
            .HasConversion(
                type => type.ToString().ToUpperInvariant(),
                value => Enum.Parse<CarTypeDb>(value, ignoreCase: true));

        builder.Property(c => c.Availability).HasColumnName("availability").IsRequired();

        builder.HasData(new CarDb
        {
            Id = 1,
            CarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab"),
            Brand = "Mercedes Benz",
            Model = "GLA 250",
            RegistrationNumber = "ЛО777Х799",
            Power = 249,
            Price = 3500,
            Type = CarTypeDb.Sedan,
            Availability = true,
        });

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_cars_type",
            "type IN ('SEDAN', 'SUV', 'MINIVAN', 'ROADSTER')"));
    }
}
