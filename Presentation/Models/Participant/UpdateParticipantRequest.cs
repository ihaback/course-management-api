using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.Participant;

public sealed record UpdateParticipantRequest
{
    [Required]
    public string FirstName { get; init; } = null!;

    [Required]
    public string LastName { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public string PhoneNumber { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public int ContactTypeId { get; init; }
}
