using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.PaymentMethods.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseRegistrations.Models;

public class CourseRegistration_Tests
{
    [Fact]
    public void Constructor_Should_Create_CourseRegistration_When_Parameters_Are_Valid()
    {
        var id = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var courseEventId = Guid.NewGuid();
        var registrationDate = DateTime.UtcNow;

        var courseRegistration = CourseRegistration.Reconstitute(
            id,
            participantId,
            courseEventId,
            registrationDate,
            CourseRegistrationStatus.Pending,
            PaymentMethod.Reconstitute(1, "Card"));

        Assert.NotNull(courseRegistration);
        Assert.Equal(id, courseRegistration.Id);
        Assert.Equal(participantId, courseRegistration.ParticipantId);
        Assert.Equal(courseEventId, courseRegistration.CourseEventId);
        Assert.Equal(registrationDate, courseRegistration.RegistrationDate);
        Assert.Equal(CourseRegistrationStatus.Pending, courseRegistration.Status);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Id_Is_Empty()
    {
        var participantId = Guid.NewGuid();
        var courseEventId = Guid.NewGuid();

        var ex = Assert.Throws<ArgumentException>(() =>
            CourseRegistration.Reconstitute(Guid.Empty, participantId, courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice")));

        Assert.Equal("id", ex.ParamName);
        Assert.Contains("ID cannot be empty", ex.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Status_Is_Null()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null!, PaymentMethod.Reconstitute(3, "Cash")));
        Assert.Equal("status", ex.ParamName);
    }

    [Fact]
    public void Constructor_Should_Throw_When_PaymentMethod_Is_Null()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Paid, null!));

        Assert.Equal("paymentMethod", ex.ParamName);
    }

    public static IEnumerable<object[]> ValidStatusAndPaymentData =>
    [
        [CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card")],
        [CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice")],
        [CourseRegistrationStatus.Cancelled, PaymentMethod.Reconstitute(3, "Cash")],
        [CourseRegistrationStatus.Refunded, PaymentMethod.Reconstitute(1, "Card")]
    ];

    [Theory]
    [MemberData(nameof(ValidStatusAndPaymentData))]
    public void Constructor_Should_Accept_All_Statuses(CourseRegistrationStatus status, PaymentMethod payment)
    {
        var registration = CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, status, payment);

        Assert.Equal(status, registration.Status);
        Assert.Equal(payment, registration.PaymentMethod);
    }

    [Fact]
    public void Properties_Should_Be_ReadOnly()
    {
        var registration = CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(1, "Card"));

        Assert.Equal(registration.Id, registration.Id);
        Assert.Equal(registration.ParticipantId, registration.ParticipantId);
        Assert.Equal(registration.CourseEventId, registration.CourseEventId);
        Assert.Equal(registration.RegistrationDate, registration.RegistrationDate);
        Assert.Equal(registration.Status, registration.Status);
    }

    [Fact]
    public void Constructor_Allows_Past_Dates()
    {
        var date = DateTime.UtcNow.AddDays(-10);

        var registration = CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), date, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(3, "Cash"));

        Assert.Equal(date, registration.RegistrationDate);
    }

    [Fact]
    public void Update_Should_Change_Values_When_Input_Is_Valid()
    {
        var registration = CourseRegistration.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            CourseRegistrationStatus.Pending,
            PaymentMethod.Reconstitute(1, "Card"));

        var newParticipantId = Guid.NewGuid();
        var newCourseEventId = Guid.NewGuid();
        var newDate = DateTime.UtcNow.AddDays(1);

        registration.Update(
            newParticipantId,
            newCourseEventId,
            newDate,
            CourseRegistrationStatus.Paid,
            PaymentMethod.Reconstitute(2, "Invoice"));

        Assert.Equal(newParticipantId, registration.ParticipantId);
        Assert.Equal(newCourseEventId, registration.CourseEventId);
        Assert.Equal(newDate, registration.RegistrationDate);
        Assert.Equal(CourseRegistrationStatus.Paid, registration.Status);
        Assert.Equal(PaymentMethod.Reconstitute(2, "Invoice"), registration.PaymentMethod);
    }

    [Fact]
    public void Update_Should_Throw_When_ParticipantId_Is_Empty()
    {
        var registration = CourseRegistration.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            CourseRegistrationStatus.Pending,
            PaymentMethod.Reconstitute(1, "Card"));

        var ex = Assert.Throws<ArgumentException>(() => registration.Update(
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow,
            CourseRegistrationStatus.Paid,
            PaymentMethod.Reconstitute(2, "Invoice")));

        Assert.Equal("participantId", ex.ParamName);
    }
}
