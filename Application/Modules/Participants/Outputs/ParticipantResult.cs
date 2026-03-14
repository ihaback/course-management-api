namespace Backend.Application.Modules.Participants.Outputs;

public sealed record ParticipantLookupItem(int Id, string Name);

public sealed record ParticipantDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    ParticipantLookupItem ContactType
);

