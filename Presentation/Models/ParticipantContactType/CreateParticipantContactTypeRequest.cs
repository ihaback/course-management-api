using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.ParticipantContactType;

public sealed record CreateParticipantContactTypeRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
