namespace Backend.Application.Modules.Courses.Inputs;

public sealed record CreateCourseInput
(
    string Title,
    string Description,
    int DurationInDays
);
