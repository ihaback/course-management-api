using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.InstructorRole;

public sealed record CreateInstructorRoleRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
