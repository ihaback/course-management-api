namespace Backend.Application.Modules.Participants.Inputs;

public sealed record CreateParticipantInput(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    int ContactTypeId
);
