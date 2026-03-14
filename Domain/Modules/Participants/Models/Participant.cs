using Backend.Domain.Common.ValueObjects;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.Participants.Models;

public sealed class Participant
{
    public Guid Id { get; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public ParticipantContactType ContactType { get; private set; } = null!;

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private Participant(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        ParticipantContactType? contactType = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", nameof(id));

        Id = id;
        SetValues(firstName, lastName, email, phoneNumber, contactType);
    }

    public static Participant Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        ParticipantContactType? contactType = null)
        => new(Guid.NewGuid(), firstName, lastName,
            Email.Create(email, nameof(email)),
            PhoneNumber.Create(phoneNumber, nameof(phoneNumber)),
            contactType);

    public static Participant Reconstitute(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        ParticipantContactType? contactType = null)
        => new(id, firstName, lastName,
            Email.Create(email, nameof(email)),
            PhoneNumber.Create(phoneNumber, nameof(phoneNumber)),
            contactType);

    public void Update(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        ParticipantContactType? contactType = null)
    {
        SetValues(firstName, lastName,
            Email.Create(email, nameof(email)),
            PhoneNumber.Create(phoneNumber, nameof(phoneNumber)),
            contactType);
    }

    private void SetValues(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        ParticipantContactType? contactType)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty or whitespace.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty or whitespace.", nameof(lastName));

        var resolvedContactType = contactType ?? ParticipantContactType.Reconstitute(1, "Primary");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
        PhoneNumber = phoneNumber;
        ContactType = resolvedContactType;
    }
}
