using System.ComponentModel.DataAnnotations;

namespace Backend.Presentation.API.Models.CourseEvent;

public sealed record CreateCourseEventRequest
{
    public Guid CourseId { get; init; }

    public DateTime EventDate { get; init; }

    public decimal Price { get; init; }

    [Range(1, int.MaxValue)]
    public int Seats { get; init; }

    [Range(1, int.MaxValue)]
    public int CourseEventTypeId { get; init; }

    [Range(1, int.MaxValue)]
    public int VenueTypeId { get; init; }
}
