using Cars.Domain.Enums;
using Cars.Domain.Exceptions;

namespace Cars.Domain.Models;

public class Car
{
    public const int BrandMaxLength = 80;
    public const int ModelMaxLength = 80;
    public const int RegistrationNumberMaxLength = 20;
    public Guid CarUid { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public string RegistrationNumber { get; private set; }
    public int Power { get; private set; }
    public int Price { get; private set; }
    public CarType Type { get; private set; }
    public bool Availability { get; private set; }

    private Car(
        Guid carUid, 
        string brand, 
        string model, 
        string registrationNumber, 
        int power, 
        int price, 
        CarType type, 
        bool availability)
    {
        Validate(carUid, brand, model, registrationNumber, power, price);
        CarUid = carUid;
        Brand = brand;
        Model = model;
        RegistrationNumber = registrationNumber;
        Power = power;
        Price = price;
        Type = type;
        Availability = availability;
    }

    private static void Validate(
        Guid carUid,
        string brand,
        string model, 
        string registrationNumber,
        int power, 
        int price)
    {
        CheckUuid(carUid);
        CheckString(brand, "Brand", BrandMaxLength);
        CheckString(model, "Model", ModelMaxLength);
        CheckString(registrationNumber, "RegistrationNumber", RegistrationNumberMaxLength);
        CheckPower(power);
        CheckPrice(price);
    }

    public static Car Create(
        string brand,
        string model,
        string registrationNumber,
        int power,
        int price,
        CarType type)
    {
        return new Car(Guid.NewGuid(), brand, model, registrationNumber, power, price, type, true);
    }

    public static Car Restore(
        Guid carUid,
        string brand,
        string model,
        string registrationNumber,
        int power,
        int price,
        CarType type,
        bool availability)
    {
        return new Car(carUid, brand, model, registrationNumber, power, price, type, availability);
    }

    public void Reserve()
    {
        if (!Availability)
            throw new CarUnavailableException($"Car {CarUid} is already reserved");
        Availability = false;
    }

    public void Release()
    {
        Availability = true;
    }

    private static void CheckUuid(Guid uuid)
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

    private static void CheckPower(int power)
    {
        if (power < 0)
            throw new DomainValidationException($"Power should not be negative");
    }

    private static void CheckPrice(int price)
    {
        if (price < 0)
            throw new DomainValidationException($"Id should not be negative");
    }
}
