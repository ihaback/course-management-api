using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.Instructor;

public sealed record UpdateInstructorRequest
{
    [Required]
    public string Name { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public int InstructorRoleId { get; init; }
}
