namespace Backend.Application.Modules.CourseEvents.Outputs;

public sealed record CourseEventLookupItem(int Id, string Name);

public sealed record CourseEventDetails(
    Guid Id,
    Guid CourseId,
    DateTime EventDate,
    decimal Price,
    int Seats,
    CourseEventLookupItem CourseEventType,
    CourseEventLookupItem VenueType
);

