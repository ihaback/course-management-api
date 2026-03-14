using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.VenueType;

public sealed record UpdateVenueTypeRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
