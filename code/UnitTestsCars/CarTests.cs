using Cars.Domain.Enums;
using Cars.Domain.Exceptions;
using Cars.Domain.Models;
using FluentAssertions;
using Xunit;

namespace UnitTestsCars;

public class CarTests
{
    private const string ValidBrand = "Mercedes Benz";
    private const string ValidModel = "GLA 250";
    private const string ValidRegistrationNumber = "ЛО777Х799";
    private const int ValidPower = 249;
    private const int ValidPrice = 3500;
    private const CarType ValidType = CarType.Sedan;

    private static Car CreateValidCar()
        => Car.Create(ValidBrand, ValidModel, ValidRegistrationNumber, ValidPower, ValidPrice, ValidType);

    // успешное создание

    [Fact]
    public void Create_WithValidData_SetsAllPropertiesAndMakesCarAvailable()
    {
        var car = CreateValidCar();

        car.CarUid.Should().NotBeEmpty();
        car.Brand.Should().Be(ValidBrand);
        car.Model.Should().Be(ValidModel);
        car.RegistrationNumber.Should().Be(ValidRegistrationNumber);
        car.Power.Should().Be(ValidPower);
        car.Price.Should().Be(ValidPrice);
        car.Type.Should().Be(ValidType);
        car.Availability.Should().BeTrue();
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDifferentUids()
    {
        var first = CreateValidCar();
        var second = CreateValidCar();

        first.CarUid.Should().NotBe(second.CarUid);
    }

    // валидация строковых полей

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingOrWhitespaceBrand_Throws(string? invalidBrand)
    {
        var act = () => Car.Create(invalidBrand!, ValidModel, ValidRegistrationNumber, ValidPower, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Brand*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingOrWhitespaceModel_Throws(string? invalidModel)
    {
        var act = () => Car.Create(ValidBrand, invalidModel!, ValidRegistrationNumber, ValidPower, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Model*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingOrWhitespaceRegistrationNumber_Throws(string? invalidNumber)
    {
        var act = () => Car.Create(ValidBrand, ValidModel, invalidNumber!, ValidPower, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*RegistrationNumber*");
    }

    [Fact]
    public void Create_WithBrandAtMaxLength_IsAllowed()
    {
        var maxBrand = new string('a', Car.BrandMaxLength);

        var car = Car.Create(maxBrand, ValidModel, ValidRegistrationNumber, ValidPower, ValidPrice, ValidType);

        car.Brand.Should().Be(maxBrand);
    }

    [Fact]
    public void Create_WithBrandExceedingMaxLength_Throws()
    {
        var tooLongBrand = new string('a', Car.BrandMaxLength + 1);

        var act = () => Car.Create(tooLongBrand, ValidModel, ValidRegistrationNumber, ValidPower, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage($"*{Car.BrandMaxLength}*");
    }

    [Fact]
    public void Create_WithRegistrationNumberExceedingMaxLength_Throws()
    {
        var tooLongNumber = new string('a', Car.RegistrationNumberMaxLength + 1);

        var act = () => Car.Create(ValidBrand, ValidModel, tooLongNumber, ValidPower, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage($"*{Car.RegistrationNumberMaxLength}*");
    }

    // валидация чисел

    [Fact]
    public void Create_WithNegativePower_Throws()
    {
        var act = () => Car.Create(ValidBrand, ValidModel, ValidRegistrationNumber, -1, ValidPrice, ValidType);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Power*");
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Car.Create(ValidBrand, ValidModel, ValidRegistrationNumber, ValidPower, -1, ValidType);

        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Create_WithZeroPowerAndPrice_IsAllowed()
    {
        var car = Car.Create(ValidBrand, ValidModel, ValidRegistrationNumber, 0, 0, ValidType);

        car.Power.Should().Be(0);
        car.Price.Should().Be(0);
    }

    // восстановление из базы

    [Fact]
    public void Restore_SetsProvidedFields()
    {
        var carUid = Guid.NewGuid();

        var car = Car.Restore(carUid, ValidBrand, ValidModel, ValidRegistrationNumber, ValidPower, ValidPrice,
            CarType.Roadster, availability: false);

        car.CarUid.Should().Be(carUid);
        car.Type.Should().Be(CarType.Roadster);
        car.Availability.Should().BeFalse();
    }

    [Fact]
    public void Restore_WithEmptyUid_Throws()
    {
        var act = () => Car.Restore(Guid.Empty, ValidBrand, ValidModel, ValidRegistrationNumber, ValidPower,
            ValidPrice, ValidType, availability: true);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Uuid*");
    }

    // резервирование и снятие резерва

    [Fact]
    public void Reserve_OnAvailableCar_MakesCarUnavailable()
    {
        var car = CreateValidCar();

        car.Reserve();

        car.Availability.Should().BeFalse();
    }

    [Fact]
    public void Reserve_OnReservedCar_Throws()
    {
        var car = CreateValidCar();
        car.Reserve();

        var act = () => car.Reserve();

        act.Should().Throw<CarUnavailableException>()
            .WithMessage($"*{car.CarUid}*");
    }

    [Fact]
    public void Release_OnReservedCar_MakesCarAvailable()
    {
        var car = CreateValidCar();
        car.Reserve();

        car.Release();

        car.Availability.Should().BeTrue();
    }

    [Fact]
    public void Release_OnAvailableCar_KeepsCarAvailable()
    {
        var car = CreateValidCar();

        car.Release();

        car.Availability.Should().BeTrue();
    }

    [Fact]
    public void Reserve_AfterRelease_IsAllowed()
    {
        var car = CreateValidCar();
        car.Reserve();
        car.Release();

        car.Reserve();

        car.Availability.Should().BeFalse();
    }
}
