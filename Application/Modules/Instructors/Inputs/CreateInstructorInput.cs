namespace Backend.Application.Modules.Instructors.Inputs;

public sealed record CreateInstructorInput(
    string Name,
    int InstructorRoleId = 1
);
