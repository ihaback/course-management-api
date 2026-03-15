using Backend.Application.Common;
using Backend.Application.Modules.Courses;
using Backend.Application.Modules.Courses.Inputs;
using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.Courses.Contracts;
using Backend.Domain.Modules.Courses.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.Courses;

public class CourseService_Tests
{
    [Fact]
    public async Task CreateCourseAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var expectedCourse = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 5);

        mockRepo.AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(expectedCourse);

        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "Test Description", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Course", result.Value.Title);
        Assert.Equal("Test Description", result.Value.Description);
        Assert.Equal(5, result.Value.DurationInDays);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<Course>(c => c.Title == "Test Course" && c.Description == "Test Description" && c.DurationInDays == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);

        // Act
        var result = await service.CreateCourseAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_Title_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("", "Test Description", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_Title_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("   ", "Test Description", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_Description_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_Description_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "   ", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_DurationInDays_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "Test Description", 0);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_BadRequest_When_DurationInDays_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "Test Description", -5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Course>(new Exception("Database error")));

        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput("Test Course", "Test Description", 5);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the course", result.ErrorMessage);
    }

    [Theory]
    [InlineData("C# Basics", "Learn C#", 10)]
    [InlineData("Advanced ASP.NET", "Master ASP.NET Core", 20)]
    [InlineData("Entity Framework", "Database access with EF", 15)]
    public async Task CreateCourseAsync_Should_Create_Course_With_Various_Valid_Inputs(
        string title, string description, int duration)
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var expectedCourse = Course.Reconstitute(Guid.NewGuid(), title, description, duration);

        mockRepo.AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(expectedCourse);

        var service = new CourseService(mockRepo);
        var input = new CreateCourseInput(title, description, duration);

        // Act
        var result = await service.CreateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(title, result.Value.Title);
        Assert.Equal(description, result.Value.Description);
        Assert.Equal(duration, result.Value.DurationInDays);
    }

    [Fact]
    public async Task CreateCourseAsync_Should_Generate_Unique_Guid_For_Each_Course()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var capturedGuids = new List<Guid>();

        mockRepo.AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var course = callInfo.Arg<Course>();
                capturedGuids.Add(course.Id);
                return course;
            });

        var service = new CourseService(mockRepo);
        var input1 = new CreateCourseInput("Course 1", "Description 1", 5);
        var input2 = new CreateCourseInput("Course 2", "Description 2", 10);

        // Act
        await service.CreateCourseAsync(input1, CancellationToken.None);
        await service.CreateCourseAsync(input2, CancellationToken.None);

        // Assert
        Assert.Equal(2, capturedGuids.Count);
        Assert.NotEqual(capturedGuids[0], capturedGuids[1]);
        Assert.NotEqual(Guid.Empty, capturedGuids[0]);
        Assert.NotEqual(Guid.Empty, capturedGuids[1]);
    }

    [Fact]
    public void CourseService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CourseService(null!));
    }

    #region GetAllCoursesAsync Tests

    [Fact]
    public async Task GetAllCoursesAsync_Should_Return_All_Courses_When_Courses_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courses = new List<Course>
        {
            Course.Reconstitute(Guid.NewGuid(), "Course 1", "Description 1", 10),
            Course.Reconstitute(Guid.NewGuid(), "Course 2", "Description 2", 20),
            Course.Reconstitute(Guid.NewGuid(), "Course 3", "Description 3", 15)
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(courses);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetAllCoursesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllCoursesAsync_Should_Return_Empty_List_When_No_Courses_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Course>());

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetAllCoursesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllCoursesAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<Course>>(new Exception("Database connection failed")));

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetAllCoursesAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving courses", result.ErrorMessage);
    }

    #endregion

    #region GetCourseByIdAsync Tests

    [Fact]
    public async Task GetCourseByIdAsync_Should_Return_Course_When_Course_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var course = Course.Reconstitute(courseId, "Test Course", "Test Description", 10);
        var courseWithEvents = new CourseWithEvents(course, new List<CourseEvent>());

        mockRepo.GetByIdWithEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetCourseByIdAsync(courseId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(courseId, result.Value.Course.Id);
        Assert.Equal("Test Course", result.Value.Course.Title);

        await mockRepo.Received(1).GetByIdWithEventsAsync(courseId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_Return_NotFound_When_Course_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();

        mockRepo.GetByIdWithEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns((CourseWithEvents)null!);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetCourseByIdAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course with ID '{courseId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_Return_BadRequest_When_CourseId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetCourseByIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdWithEventsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();

        mockRepo.GetByIdWithEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseWithEvents?>(new Exception("Database error")));

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.GetCourseByIdAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the course", result.ErrorMessage);
    }

    #endregion

    #region UpdateCourseAsync Tests

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Old Title", "Old Description", 5);
        var courseWithEvents = existingCourse;
        var updatedCourse = Course.Reconstitute(courseId, "Updated Title", "Updated Description", 10);

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(updatedCourse);

        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(courseId, "Updated Title", "Updated Description", 10);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Title", result.Value.Title);
        Assert.Equal("Updated Description", result.Value.Description);
        Assert.Equal(10, result.Value.DurationInDays);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<Guid>(id => id == courseId),
            Arg.Is<Course>(c => c.Id == courseId && c.Title == "Updated Title" && c.Description == "Updated Description" && c.DurationInDays == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);

        // Act
        var result = await service.UpdateCourseAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_BadRequest_When_CourseId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(Guid.Empty, "Test Title", "Test Description", 5);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_BadRequest_When_Title_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Course.Reconstitute(Guid.NewGuid(), "Old Title", "Old Description", 5));
        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(Guid.NewGuid(), "", "Test Description", 5);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_BadRequest_When_Description_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Course.Reconstitute(Guid.NewGuid(), "Old Title", "Old Description", 5));
        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(Guid.NewGuid(), "Test Title", "", 5);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_BadRequest_When_DurationInDays_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Course.Reconstitute(Guid.NewGuid(), "Old Title", "Old Description", 5));
        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(Guid.NewGuid(), "Test Title", "Test Description", 0);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_NotFound_When_Course_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns((Course?)null);

        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(courseId, "Test Title", "Test Description", 5);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course with ID '{courseId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_Conflict_When_Concurrency_Exception_Occurs()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Old Title", "Old Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Course?>(new InvalidOperationException("Course was modified by another user")));

        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(courseId, "Updated Title", "Updated Description", 10);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Old Title", "Old Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Course>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Course?>(new Exception("Database error")));

        var service = new CourseService(mockRepo);
        var input = new UpdateCourseInput(courseId, "Updated Title", "Updated Description", 10);

        // Act
        var result = await service.UpdateCourseAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the course", result.ErrorMessage);
    }

    #endregion

    #region DeleteCourseAsync Tests

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_Success_When_Course_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Test Course", "Test Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.HasCourseEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(courseId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(courseId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_BadRequest_When_CourseId_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_NotFound_When_Course_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns((Course?)null);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"Course with ID '{courseId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_Conflict_When_Course_Has_Associated_Events()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Test Course", "Test Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.HasCourseEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);        Assert.Contains("Cannot delete course", result.ErrorMessage);
        Assert.Contains("has associated course events", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Test Course", "Test Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.HasCourseEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the course", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseRepository>();
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Reconstitute(courseId, "Test Course", "Test Description", 5);
        var courseWithEvents = existingCourse;

        mockRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(courseWithEvents);

        mockRepo.HasCourseEventsAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(courseId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = new CourseService(mockRepo);

        // Act
        var result = await service.DeleteCourseAsync(courseId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);    }

    #endregion
}

