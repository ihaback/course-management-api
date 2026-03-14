namespace Backend.Application.Modules.Locations.Inputs;

public sealed record CreateLocationInput(
    string StreetName,
    string PostalCode,
    string City
);
