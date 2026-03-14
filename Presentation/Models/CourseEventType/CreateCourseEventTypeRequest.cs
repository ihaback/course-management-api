using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.CourseEventType;

public sealed record CreateCourseEventTypeRequest
{
    [Required]
    public string Name { get; init; } = null!;
}
