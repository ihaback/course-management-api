using Backend.Domain.Modules.CourseEvents.Models;

namespace Backend.Domain.Modules.Courses.Models;

public sealed class CourseWithEvents
{
    public Course Course { get; init; }
    public IReadOnlyList<CourseEvent> Events { get; init; }

    public CourseWithEvents(Course course, IReadOnlyList<CourseEvent> events)
    {
        Course = course ?? throw new ArgumentNullException(nameof(course));
        Events = events ?? throw new ArgumentNullException(nameof(events));

        if (Events.Any(e => e is null))
            throw new ArgumentException("Events collection cannot contain null items.", nameof(events));

        if (Events.Any(e => e.CourseId != Course.Id))
            throw new ArgumentException("All events must belong to the same course.", nameof(events));
    }
}
