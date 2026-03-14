namespace Backend.Application.Modules.Courses.Inputs;

public sealed record UpdateCourseInput
(
    Guid Id,
    string Title,
    string Description,
    int DurationInDays
);
