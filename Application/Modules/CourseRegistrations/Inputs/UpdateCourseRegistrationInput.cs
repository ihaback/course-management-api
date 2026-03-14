namespace Backend.Application.Modules.CourseRegistrations.Inputs;

public sealed record UpdateCourseRegistrationInput(
    Guid Id,
    Guid ParticipantId,
    Guid CourseEventId,
    int StatusId,
    int PaymentMethodId
);

