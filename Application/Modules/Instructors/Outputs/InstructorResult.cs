namespace Backend.Application.Modules.Instructors.Outputs;

public sealed record InstructorLookupItem(int Id, string Name);

public sealed record InstructorDetails(
    Guid Id,
    string Name,
    InstructorLookupItem InstructorRole
);

