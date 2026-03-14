namespace Backend.Application.Modules.Participants.Inputs;

public sealed record UpdateParticipantInput(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    int ContactTypeId
);
