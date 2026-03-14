namespace Backend.Application.Modules.Locations.Inputs;

public sealed record UpdateLocationInput(
    int Id,
    string StreetName,
    string PostalCode,
    string City
);
