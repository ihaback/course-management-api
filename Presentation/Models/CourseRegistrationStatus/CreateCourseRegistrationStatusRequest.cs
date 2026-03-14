using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.CourseRegistrationStatus;

public sealed record CreateCourseRegistrationStatusRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
