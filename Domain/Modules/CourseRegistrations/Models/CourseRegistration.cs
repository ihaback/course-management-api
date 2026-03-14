using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using PaymentMethodModel = Backend.Domain.Modules.PaymentMethods.Models.PaymentMethod;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.CourseRegistrations.Models;

public sealed class CourseRegistration
{
    public Guid Id { get; }
    public Guid ParticipantId { get; private set; }
    public Guid CourseEventId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public CourseRegistrationStatus Status { get; private set; }
    public PaymentMethodModel PaymentMethod { get; private set; } = null!;

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private CourseRegistration(
        Guid id,
        Guid participantId,
        Guid courseEventId,
        DateTime registrationDate,
        CourseRegistrationStatus status,
        PaymentMethodModel paymentMethod)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", nameof(id));

        Id = id;
        SetValues(participantId, courseEventId, registrationDate, status, paymentMethod);
    }

    public static CourseRegistration Create(
        Guid participantId,
        Guid courseEventId,
        DateTime registrationDate,
        CourseRegistrationStatus status,
        PaymentMethodModel paymentMethod)
        => new(Guid.NewGuid(), participantId, courseEventId, registrationDate, status, paymentMethod);

    public static CourseRegistration Reconstitute(
        Guid id,
        Guid participantId,
        Guid courseEventId,
        DateTime registrationDate,
        CourseRegistrationStatus status,
        PaymentMethodModel paymentMethod)
        => new(id, participantId, courseEventId, registrationDate, status, paymentMethod);

    public void Update(
        Guid participantId,
        Guid courseEventId,
        DateTime registrationDate,
        CourseRegistrationStatus status,
        PaymentMethodModel paymentMethod)
    {
        SetValues(participantId, courseEventId, registrationDate, status, paymentMethod);
    }

    [MemberNotNull(nameof(Status))]
    private void SetValues(
        Guid participantId,
        Guid courseEventId,
        DateTime registrationDate,
        CourseRegistrationStatus status,
        PaymentMethodModel paymentMethod)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant ID cannot be empty.", nameof(participantId));

        if (courseEventId == Guid.Empty)
            throw new ArgumentException("Course event ID cannot be empty.", nameof(courseEventId));

        if (registrationDate == default)
            throw new ArgumentException("Registration date must be specified.", nameof(registrationDate));

        ArgumentNullException.ThrowIfNull(status);
        ArgumentNullException.ThrowIfNull(paymentMethod);

        ParticipantId = participantId;
        CourseEventId = courseEventId;
        RegistrationDate = registrationDate;
        Status = status;
        PaymentMethod = PaymentMethodModel.Reconstitute(paymentMethod.Id, paymentMethod.Name);
    }
}
