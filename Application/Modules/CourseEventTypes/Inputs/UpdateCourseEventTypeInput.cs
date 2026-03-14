namespace Backend.Application.Modules.CourseEventTypes.Inputs;

public sealed record UpdateCourseEventTypeInput(
    int Id,
    string Name
);
