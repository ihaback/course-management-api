using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.CourseRegistration;

public sealed record UpdateCourseRegistrationRequest
{
    public Guid ParticipantId { get; init; }

    public Guid CourseEventId { get; init; }

    [Range(0, int.MaxValue)]
    public int StatusId { get; init; }

    [Range(0, int.MaxValue)]
    public int PaymentMethodId { get; init; }
}
