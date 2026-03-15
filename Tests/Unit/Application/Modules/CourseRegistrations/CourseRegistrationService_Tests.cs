using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrations;
using Backend.Application.Modules.CourseRegistrations.Inputs;
using Backend.Domain.Modules.CourseEvents.Contracts;
using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrations.Contracts;
using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Domain.Modules.Participants.Contracts;
using Backend.Domain.Modules.Participants.Models;
using Backend.Domain.Modules.PaymentMethods.Contracts;
using Backend.Domain.Modules.PaymentMethods.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.CourseRegistrations;

public class CourseRegistrationService_Tests
{
    private static CourseRegistrationService CreateService(
        ICourseRegistrationRepository? registrationRepository = null,
        IParticipantRepository? participantRepository = null,
        ICourseEventRepository? courseEventRepository = null,
        ICourseRegistrationStatusRepository? statusRepository = null,
        IPaymentMethodRepository? paymentMethodRepository = null)
    {
        var regRepo = registrationRepository ?? Substitute.For<ICourseRegistrationRepository>();
        var pRepo = participantRepository ?? Substitute.For<IParticipantRepository>();
        var ceRepo = courseEventRepository ?? Substitute.For<ICourseEventRepository>();
        var statusRepo = statusRepository ?? Substitute.For<ICourseRegistrationStatusRepository>();
        var payRepo = paymentMethodRepository ?? Substitute.For<IPaymentMethodRepository>();

        pRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var id = ci.Arg<Guid>();
                return id == Guid.Empty ? null : Participant.Reconstitute(id, "A", "B", "a@b.com", "123", ParticipantContactType.Reconstitute(1, "Primary"));
            });

        ceRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var id = ci.Arg<Guid>();
                return id == Guid.Empty ? null : CourseEvent.Reconstitute(id, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 10, 5, VenueType.Reconstitute(1, "InPerson"), CourseEventType.Reconstitute(1, "Type"));
            });

        statusRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var id = ci.Arg<int>();
                return id switch
                {
                    0 => CourseRegistrationStatus.Pending,
                    1 => CourseRegistrationStatus.Paid,
                    2 => CourseRegistrationStatus.Cancelled,
                    3 => CourseRegistrationStatus.Refunded,
                    _ => null
                };
            });

        payRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var id = ci.Arg<int>();
                return id <= 0 ? null : PaymentMethod.Reconstitute(id, id == 2 ? "Invoice" : "Card");
            });

        return new CourseRegistrationService(regRepo, pRepo, ceRepo, statusRepo, payRepo);
    }

    #region CreateCourseRegistrationAsync Tests

    [Fact]
    public async Task CreateCourseRegistrationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var participantId = Guid.NewGuid();
        var courseEventId = Guid.NewGuid();
        var expectedRegistration = CourseRegistration.Reconstitute(Guid.NewGuid(), participantId, courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(expectedRegistration);

        var service = CreateService(mockRepo);
        var input = new CreateCourseRegistrationInput(participantId, courseEventId, 0, 1);

        // Act
        var result = await service.CreateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(participantId, result.Value.ParticipantId);
        Assert.Equal(courseEventId, result.Value.CourseEventId);
        Assert.Equal(CourseRegistrationStatus.Pending, result.Value.Status);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<CourseRegistration>(cr => cr.ParticipantId == participantId && cr.CourseEventId == courseEventId && cr.Status == CourseRegistrationStatus.Pending && cr.PaymentMethod.Equals(PaymentMethod.Reconstitute(1, "Card"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.CreateCourseRegistrationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_Should_Return_BadRequest_When_ParticipantId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);
        var input = new CreateCourseRegistrationInput(Guid.Empty, Guid.NewGuid(), 0, 1);

        // Act
        var result = await service.CreateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Participant ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_Should_Return_BadRequest_When_CourseEventId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);
        var input = new CreateCourseRegistrationInput(Guid.NewGuid(), Guid.Empty, 0, 1);

        // Act
        var result = await service.CreateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Course event ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        mockRepo.AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistration>(new Exception("Database error")));

        var service = CreateService(mockRepo);
        var input = new CreateCourseRegistrationInput(Guid.NewGuid(), Guid.NewGuid(), 0, 1);

        // Act
        var result = await service.CreateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the course registration", result.ErrorMessage);
    }

    public static IEnumerable<object[]> CreateRegistrationStatusAndPaymentData =>
    [
        [CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice")],
        [CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card")]
    ];

    [Theory]
    [MemberData(nameof(CreateRegistrationStatusAndPaymentData))]
    public async Task CreateCourseRegistrationAsync_Should_Create_Registration_With_Various_Statuses(CourseRegistrationStatus status, PaymentMethod payment)
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var participantId = Guid.NewGuid();
        var courseEventId = Guid.NewGuid();
        var expectedRegistration = CourseRegistration.Reconstitute(Guid.NewGuid(), participantId, courseEventId, DateTime.UtcNow, status, payment);

        mockRepo.AddAsync(Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(expectedRegistration);

        var service = CreateService(mockRepo);
        var input = new CreateCourseRegistrationInput(participantId, courseEventId, status.Id, payment.Id);

        // Act
        var result = await service.CreateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(status, result.Value.Status);
        Assert.Equal(payment, result.Value.PaymentMethod);
    }

    [Fact]
    public void CourseRegistrationService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CourseRegistrationService(null!, Substitute.For<IParticipantRepository>(), Substitute.For<ICourseEventRepository>(), Substitute.For<ICourseRegistrationStatusRepository>(), Substitute.For<IPaymentMethodRepository>()));
    }

    #endregion

    #region GetAllCourseRegistrationsAsync Tests

    [Fact]
    public async Task GetAllCourseRegistrationsAsync_Should_Return_All_Registrations_When_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrations = new List<CourseRegistration>
        {
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card")),
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice")),
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"))
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(registrations);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllCourseRegistrationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllCourseRegistrationsAsync_Should_Return_Empty_List_When_No_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CourseRegistration>());

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllCourseRegistrationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllCourseRegistrationsAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<CourseRegistration>>(new Exception("Database connection failed")));

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllCourseRegistrationsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving course registrations", result.ErrorMessage);
    }

    #endregion

    #region GetCourseRegistrationByIdAsync Tests

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_Should_Return_Registration_When_Registration_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var registration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(registration);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationByIdAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(registrationId, result.Value.Id);

        await mockRepo.Received(1).GetByIdAsync(registrationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns((CourseRegistration)null!);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationByIdAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course registration with ID '{registrationId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_Should_Return_BadRequest_When_RegistrationId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationByIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistration?>(new Exception("Database error")));

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationByIdAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the course registration", result.ErrorMessage);
    }

    #endregion

    #region GetCourseRegistrationsByParticipantIdAsync Tests

    [Fact]
    public async Task GetCourseRegistrationsByParticipantIdAsync_Should_Return_Registrations_When_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var participantId = Guid.NewGuid();
        var registrations = new List<CourseRegistration>
        {
            CourseRegistration.Reconstitute(Guid.NewGuid(), participantId, Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card")),
            CourseRegistration.Reconstitute(Guid.NewGuid(), participantId, Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice"))
        };

        mockRepo.GetCourseRegistrationsByParticipantIdAsync(participantId, Arg.Any<CancellationToken>())
            .Returns(registrations);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByParticipantIdAsync(participantId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());

        await mockRepo.Received(1).GetCourseRegistrationsByParticipantIdAsync(participantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationsByParticipantIdAsync_Should_Return_Empty_List_When_No_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var participantId = Guid.NewGuid();

        mockRepo.GetCourseRegistrationsByParticipantIdAsync(participantId, Arg.Any<CancellationToken>())
            .Returns(new List<CourseRegistration>());

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByParticipantIdAsync(participantId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetCourseRegistrationsByParticipantIdAsync_Should_Return_BadRequest_When_ParticipantId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByParticipantIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("Participant ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetCourseRegistrationsByParticipantIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationsByParticipantIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var participantId = Guid.NewGuid();

        mockRepo.GetCourseRegistrationsByParticipantIdAsync(participantId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<CourseRegistration>>(new Exception("Database error")));

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByParticipantIdAsync(participantId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving course registrations", result.ErrorMessage);
    }

    #endregion

    #region GetCourseRegistrationsByCourseEventIdAsync Tests

    [Fact]
    public async Task GetCourseRegistrationsByCourseEventIdAsync_Should_Return_Registrations_When_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var courseEventId = Guid.NewGuid();
        var registrations = new List<CourseRegistration>
        {
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card")),
            CourseRegistration.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice"))
        };

        mockRepo.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, Arg.Any<CancellationToken>())
            .Returns(registrations);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());

        await mockRepo.Received(1).GetCourseRegistrationsByCourseEventIdAsync(courseEventId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationsByCourseEventIdAsync_Should_Return_Empty_List_When_No_Registrations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var courseEventId = Guid.NewGuid();

        mockRepo.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, Arg.Any<CancellationToken>())
            .Returns(new List<CourseRegistration>());

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetCourseRegistrationsByCourseEventIdAsync_Should_Return_BadRequest_When_CourseEventId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByCourseEventIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("Course event ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetCourseRegistrationsByCourseEventIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseRegistrationsByCourseEventIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var courseEventId = Guid.NewGuid();

        mockRepo.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<CourseRegistration>>(new Exception("Database error")));

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving course registrations", result.ErrorMessage);
    }

    #endregion

    #region UpdateCourseRegistrationAsync Tests

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var courseEventId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, participantId, courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));
        var updatedRegistration = CourseRegistration.Reconstitute(registrationId, participantId, courseEventId, DateTime.UtcNow, CourseRegistrationStatus.Paid, PaymentMethod.Reconstitute(2, "Invoice"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(updatedRegistration);

        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(registrationId, participantId, courseEventId, 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(CourseRegistrationStatus.Paid, result.Value.Status);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<Guid>(id => id == registrationId),
            Arg.Is<CourseRegistration>(cr => cr.Id == registrationId && cr.Status == CourseRegistrationStatus.Paid && cr.PaymentMethod.Equals(PaymentMethod.Reconstitute(2, "Invoice"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_BadRequest_When_RegistrationId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("ID cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_BadRequest_When_ParticipantId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Participant ID cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_BadRequest_When_CourseEventId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Course event ID cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns((CourseRegistration)null!);

        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(registrationId, Guid.NewGuid(), Guid.NewGuid(), 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course registration with ID '{registrationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_Conflict_When_Concurrency_Exception_Occurs()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistration?>(new InvalidOperationException("Course registration was modified by another user")));

        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(registrationId, Guid.NewGuid(), Guid.NewGuid(), 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<CourseRegistration>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseRegistration?>(new Exception("Database error")));

        var service = CreateService(mockRepo);
        var input = new UpdateCourseRegistrationInput(registrationId, Guid.NewGuid(), Guid.NewGuid(), 1, 2);

        // Act
        var result = await service.UpdateCourseRegistrationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the course registration", result.ErrorMessage);
    }

    #endregion

    #region DeleteCourseRegistrationAsync Tests

    [Fact]
    public async Task DeleteCourseRegistrationAsync_Should_Return_Success_When_Registration_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.RemoveAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.DeleteCourseRegistrationAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(registrationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_Should_Return_BadRequest_When_RegistrationId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var service = CreateService(mockRepo);

        // Act
        var result = await service.DeleteCourseRegistrationAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_Should_Return_NotFound_When_Registration_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns((CourseRegistration)null!);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.DeleteCourseRegistrationAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"Course registration with ID '{registrationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.RemoveAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = CreateService(mockRepo);

        // Act
        var result = await service.DeleteCourseRegistrationAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the course registration", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRegistrationRepository>();
        var registrationId = Guid.NewGuid();
        var existingRegistration = CourseRegistration.Reconstitute(registrationId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, CourseRegistrationStatus.Pending, PaymentMethod.Reconstitute(1, "Card"));

        mockRepo.GetByIdAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(existingRegistration);

        mockRepo.RemoveAsync(registrationId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.DeleteCourseRegistrationAsync(registrationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);    }

    #endregion
}

