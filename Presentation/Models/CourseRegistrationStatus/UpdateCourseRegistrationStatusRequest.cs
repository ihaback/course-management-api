using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.CourseRegistrationStatus;

public sealed record UpdateCourseRegistrationStatusRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
