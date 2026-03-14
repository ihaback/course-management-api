namespace Backend.Application.Modules.CourseRegistrations.Inputs;

public sealed record CreateCourseRegistrationInput(
    Guid ParticipantId,
    Guid CourseEventId,
    int StatusId,
    int PaymentMethodId
);

