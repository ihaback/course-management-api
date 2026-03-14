using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Backend.Domain.Modules.Locations.Models;

public sealed class Location
{
    public int Id { get; }
    public string StreetName { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string City { get; private set; } = null!;

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private Location(int id, string streetName, string postalCode, string city)
    {
        if (id < 0)
            throw new ArgumentException("ID must be greater than or equal to zero.", nameof(id));

        Id = id;
        SetValues(streetName, postalCode, city);
    }

    public static Location Create(string streetName, string postalCode, string city)
        => new(0, streetName, postalCode, city);

    public static Location Reconstitute(int id, string streetName, string postalCode, string city)
        => new(id, streetName, postalCode, city);

    public void Update(string streetName, string postalCode, string city)
    {
        SetValues(streetName, postalCode, city);
    }

    private void SetValues(string streetName, string postalCode, string city)
    {
        if (string.IsNullOrWhiteSpace(streetName))
            throw new ArgumentException("Street name cannot be empty or whitespace.", nameof(streetName));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty or whitespace.", nameof(postalCode));


        if (!Regex.IsMatch(postalCode.Trim(), @"^\d{5}$"))
            throw new ArgumentException(
                "Postal code must consist of exactly 5 digits with no spaces",
                nameof(postalCode));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty or whitespace.", nameof(city));

        StreetName = streetName.Trim();
        PostalCode = postalCode.Trim();
        City = city.Trim();
    }
}
