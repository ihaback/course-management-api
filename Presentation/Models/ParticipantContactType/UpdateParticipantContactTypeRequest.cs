using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.ParticipantContactType;

public sealed record UpdateParticipantContactTypeRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
