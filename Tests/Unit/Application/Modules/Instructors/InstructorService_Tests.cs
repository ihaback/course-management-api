using Backend.Application.Common;
using Backend.Application.Modules.Instructors;
using Backend.Application.Modules.Instructors.Inputs;
using Backend.Domain.Modules.InstructorRoles.Contracts;
using Backend.Domain.Modules.InstructorRoles.Models;
using Backend.Domain.Modules.Instructors.Contracts;
using Backend.Domain.Modules.Instructors.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.Instructors;

public class InstructorService_Tests
{
    private static IInstructorRoleRepository CreateRoleRepo()
    {
        var repo = Substitute.For<IInstructorRoleRepository>();
        repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci => InstructorRole.Reconstitute(ci.Arg<int>(), $"Role-{ci.Arg<int>()}"));
        return repo;
    }

    #region CreateInstructorAsync Tests

    [Fact]
    public async Task CreateInstructorAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var mockRoleRepo = Substitute.For<IInstructorRoleRepository>();
        mockRoleRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(InstructorRole.Reconstitute(1, "Lead"));

        var expectedInstructor = Instructor.Reconstitute(Guid.NewGuid(), "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>())
            .Returns(expectedInstructor);

        var service = new InstructorService(mockRepo, mockRoleRepo);
        var input = new CreateInstructorInput("Dr. John Doe");

        // Act
        var result = await service.CreateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Dr. John Doe", result.Value.Name);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<Instructor>(i => i.Name == "Dr. John Doe"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInstructorAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.CreateInstructorAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInstructorAsync_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new CreateInstructorInput("");

        // Act
        var result = await service.CreateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInstructorAsync_Should_Return_BadRequest_When_Name_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new CreateInstructorInput("   ");

        // Act
        var result = await service.CreateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInstructorAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        mockRepo.AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Instructor>(new Exception("Database error")));

        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new CreateInstructorInput("Dr. John Doe");

        // Act
        var result = await service.CreateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the instructor", result.ErrorMessage);
    }

    [Theory]
    [InlineData("Dr. Jane Smith")]
    [InlineData("Prof. Robert Johnson")]
    [InlineData("Alice Williams")]
    [InlineData("Dr. O'Brien-Smith")]
    public async Task CreateInstructorAsync_Should_Create_Instructor_With_Various_Valid_Names(string name)
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var expectedInstructor = Instructor.Reconstitute(Guid.NewGuid(), name, InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.AddAsync(Arg.Any<Instructor>(), Arg.Any<CancellationToken>())
            .Returns(expectedInstructor);

        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new CreateInstructorInput(name);

        // Act
        var result = await service.CreateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
    }

    [Fact]
    public void InstructorService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        var roleRepo = CreateRoleRepo();
        Assert.Throws<ArgumentNullException>(() => new InstructorService(null!, roleRepo));
        var instructorRepo = Substitute.For<IInstructorRepository>();
        Assert.Throws<ArgumentNullException>(() => new InstructorService(instructorRepo, null!));
    }

    #endregion

    #region GetAllInstructorsAsync Tests

    [Fact]
    public async Task GetAllInstructorsAsync_Should_Return_All_Instructors_When_Instructors_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructors = new List<Instructor>
        {
            Instructor.Reconstitute(Guid.NewGuid(), "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead")),
            Instructor.Reconstitute(Guid.NewGuid(), "Prof. Jane Smith", InstructorRole.Reconstitute(1, "Lead")),
            Instructor.Reconstitute(Guid.NewGuid(), "Dr. Robert Johnson", InstructorRole.Reconstitute(1, "Lead"))
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(instructors);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetAllInstructorsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllInstructorsAsync_Should_Return_Empty_List_When_No_Instructors_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Instructor>());

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetAllInstructorsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllInstructorsAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<Instructor>>(new Exception("Database connection failed")));

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetAllInstructorsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving instructors", result.ErrorMessage);
    }

    #endregion

    #region GetInstructorByIdAsync Tests

    [Fact]
    public async Task GetInstructorByIdAsync_Should_Return_Instructor_When_Instructor_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var instructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(instructor);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetInstructorByIdAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(instructorId, result.Value.Id);
        Assert.Equal("Dr. John Doe", result.Value.Name);

        await mockRepo.Received(1).GetByIdAsync(instructorId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInstructorByIdAsync_Should_Return_NotFound_When_Instructor_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns((Instructor)null!);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetInstructorByIdAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Instructor with ID '{instructorId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetInstructorByIdAsync_Should_Return_BadRequest_When_InstructorId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetInstructorByIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInstructorByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Instructor?>(new Exception("Database error")));

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.GetInstructorByIdAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the instructor", result.ErrorMessage);
    }

    #endregion

    #region UpdateInstructorAsync Tests

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));
        var updatedInstructor = Instructor.Reconstitute(instructorId, "Prof. John Doe", InstructorRole.Reconstitute(2, "Assistant"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Instructor>(), Arg.Any<CancellationToken>())
            .Returns(updatedInstructor);

        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(instructorId, "Prof. John Doe");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Prof. John Doe", result.Value.Name);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<Guid>(id => id == instructorId),
            Arg.Is<Instructor>(i => i.Id == instructorId && i.Name == "Prof. John Doe"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.UpdateInstructorAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<Instructor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_BadRequest_When_InstructorId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(Guid.Empty, "Dr. John Doe");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Instructor.Reconstitute(Guid.NewGuid(), "Existing", InstructorRole.Reconstitute(1, "Lead")));
        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(Guid.NewGuid(), "");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_BadRequest_When_Name_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Instructor.Reconstitute(Guid.NewGuid(), "Existing", InstructorRole.Reconstitute(1, "Lead")));
        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(Guid.NewGuid(), "   ");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_NotFound_When_Instructor_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns((Instructor)null!);

        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(instructorId, "Dr. John Doe");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Instructor with ID '{instructorId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<Instructor>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInstructorAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Instructor>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Instructor?>(new Exception("Database error")));

        var service = new InstructorService(mockRepo, CreateRoleRepo());
        var input = new UpdateInstructorInput(instructorId, "Prof. John Doe");

        // Act
        var result = await service.UpdateInstructorAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the instructor", result.ErrorMessage);
    }

    #endregion

    #region DeleteInstructorAsync Tests

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_Success_When_Instructor_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.HasCourseEventsAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(instructorId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_BadRequest_When_InstructorId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_NotFound_When_Instructor_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns((Instructor)null!);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"Instructor with ID '{instructorId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_Conflict_When_Instructor_Has_CourseEvents()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.HasCourseEventsAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);        Assert.Contains("Cannot delete instructor", result.ErrorMessage);
        Assert.Contains("assigned to course events", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.HasCourseEventsAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the instructor", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteInstructorAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<IInstructorRepository>();
        var instructorId = Guid.NewGuid();
        var existingInstructor = Instructor.Reconstitute(instructorId, "Dr. John Doe", InstructorRole.Reconstitute(1, "Lead"));

        mockRepo.GetByIdAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(existingInstructor);

        mockRepo.HasCourseEventsAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(instructorId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = new InstructorService(mockRepo, CreateRoleRepo());

        // Act
        var result = await service.DeleteInstructorAsync(instructorId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);    }

    #endregion
}

