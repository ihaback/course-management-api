namespace Backend.Application.Modules.CourseRegistrations.Outputs;

public sealed record RegistrationLookupItem(int Id, string Name);

public sealed record RegistrationGuidLookupItem(Guid Id, string Name);

public sealed record RegistrationCourseEventItem(Guid Id, DateTime? EventDate);

public sealed record CourseRegistrationDetails(
    Guid Id,
    RegistrationGuidLookupItem Participant,
    RegistrationCourseEventItem CourseEvent,
    DateTime RegistrationDate,
    RegistrationLookupItem Status,
    RegistrationLookupItem PaymentMethod
);
