using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.Location;

public sealed record UpdateLocationRequest
{
    [Required]
    public string StreetName { get; init; } = null!;

    [Required]
    public string PostalCode { get; init; } = null!;

    [Required]
    public string City { get; init; } = null!;
}
