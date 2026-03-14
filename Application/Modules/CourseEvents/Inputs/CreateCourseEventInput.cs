namespace Backend.Application.Modules.CourseEvents.Inputs;

public sealed record CreateCourseEventInput(
    Guid CourseId,
    DateTime EventDate,
    decimal Price,
    int Seats,
    int CourseEventTypeId,
    int VenueTypeId
);
