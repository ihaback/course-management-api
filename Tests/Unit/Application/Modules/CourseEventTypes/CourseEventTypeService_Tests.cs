using Backend.Application.Common;
using Backend.Application.Modules.CourseEventTypes;
using Backend.Application.Modules.CourseEventTypes.Caching;
using Backend.Application.Modules.CourseEventTypes.Inputs;
using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.CourseEventTypes.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.CourseEventTypes;

public class CourseEventTypeService_Tests
{
    private static CourseEventTypeService CreateService(ICourseEventTypeRepository repo, out ICourseEventTypeCache cache)
    {
        cache = Substitute.For<ICourseEventTypeCache>();

        cache.GetAllAsync(Arg.Any<Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>>>()(ci.Arg<CancellationToken>())!);

        cache.GetByIdAsync(Arg.Any<int>(), Arg.Any<Func<CancellationToken, Task<CourseEventType?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseEventType?>>>()(ci.Arg<CancellationToken>()));

        cache.GetByNameAsync(Arg.Any<string>(), Arg.Any<Func<CancellationToken, Task<CourseEventType?>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<CourseEventType?>>>()(ci.Arg<CancellationToken>()));

        return new CourseEventTypeService(cache, repo);
    }
    #region CreateCourseEventTypeAsync Tests

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var expectedType = CourseEventType.Reconstitute(1, "Online");

        mockRepo.AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>())
            .Returns(expectedType);

        var service = CreateService(mockRepo, out var mockCache);
        var input = new CreateCourseEventTypeInput("Online");

        // Act
        var result = await service.CreateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Online", result.Value.Name);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<CourseEventType>(t => t.Name == "Online"),
            Arg.Any<CancellationToken>());
        mockCache.Received(1).ResetEntity(expectedType);
        mockCache.Received(1).SetEntity(expectedType);
    }

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.CreateCourseEventTypeAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
        mockCache.DidNotReceive().ResetEntity(Arg.Any<CourseEventType>());
        mockCache.DidNotReceive().SetEntity(Arg.Any<CourseEventType>());
    }

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeName_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);
        var input = new CreateCourseEventTypeInput("");

        // Act
        var result = await service.CreateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Type name cannot be empty or whitespace.", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeName_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);
        var input = new CreateCourseEventTypeInput("   ");

        // Act
        var result = await service.CreateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Type name cannot be empty or whitespace.", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseEventType>(new Exception("Database error")));

        var service = CreateService(mockRepo, out var mockCache);
        var input = new CreateCourseEventTypeInput("Online");

        // Act
        var result = await service.CreateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the course event type", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeName_Already_Exists()
    {
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetCourseEventTypeByTypeNameAsync("Online", Arg.Any<CancellationToken>())
            .Returns(CourseEventType.Reconstitute(1, "Online"));

        var service = CreateService(mockRepo, out var mockCache);

        var result = await service.CreateCourseEventTypeAsync(new CreateCourseEventTypeInput("Online"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
        mockCache.DidNotReceive().ResetEntity(Arg.Any<CourseEventType>());
        mockCache.DidNotReceive().SetEntity(Arg.Any<CourseEventType>());
    }

    [Theory]
    [InlineData("Online")]
    [InlineData("In-Person")]
    [InlineData("Hybrid")]
    [InlineData("Virtual")]
    public async Task CreateCourseEventTypeAsync_Should_Create_Type_With_Various_Valid_Names(string name)
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var expectedType = CourseEventType.Reconstitute(1, name);

        mockRepo.AddAsync(Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>())
            .Returns(expectedType);

        var service = CreateService(mockRepo, out var mockCache);
        var input = new CreateCourseEventTypeInput(name);

        // Act
        var result = await service.CreateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
    }

    [Fact]
    public void CourseEventTypeService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        var cache = Substitute.For<ICourseEventTypeCache>();
        Assert.Throws<ArgumentNullException>(() => new CourseEventTypeService(cache, null!));
        Assert.Throws<ArgumentNullException>(() => new CourseEventTypeService(null!, Substitute.For<ICourseEventTypeRepository>()));
    }

    #endregion

    #region GetAllCourseEventTypesAsync Tests

    [Fact]
    public async Task GetAllCourseEventTypesAsync_Should_Return_All_Types_When_Types_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var types = new List<CourseEventType>
        {
            CourseEventType.Reconstitute(1, "Online"),
            CourseEventType.Reconstitute(2, "In-Person"),
            CourseEventType.Reconstitute(3, "Hybrid")
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(types);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetAllCourseEventTypesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        await mockCache.Received(1).GetAllAsync(
            Arg.Any<Func<CancellationToken, Task<IReadOnlyList<CourseEventType>>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllCourseEventTypesAsync_Should_Return_Empty_List_When_No_Types_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CourseEventType>());

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetAllCourseEventTypesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllCourseEventTypesAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<CourseEventType>>(new Exception("Database connection failed")));

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetAllCourseEventTypesAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving course event types", result.ErrorMessage);
    }

    #endregion

    #region GetCourseEventTypeByTypeNameAsync Tests

    [Fact]
    public async Task GetCourseEventTypeByTypeNameAsync_Should_Return_Type_When_Type_Exists()
    {
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var name = "Online";
        var courseEventType = CourseEventType.Reconstitute(1, name);

        mockRepo.GetCourseEventTypeByTypeNameAsync(name, Arg.Any<CancellationToken>())
            .Returns(courseEventType);

        var service = CreateService(mockRepo, out var mockCache);

        var result = await service.GetCourseEventTypeByTypeNameAsync(name, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);

        await mockRepo.Received(1).GetCourseEventTypeByTypeNameAsync(name, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByTypeNameAsync_Should_Return_BadRequest_When_TypeName_Is_Empty()
    {
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        var result = await service.GetCourseEventTypeByTypeNameAsync(" ", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);

        await mockRepo.DidNotReceive().GetCourseEventTypeByTypeNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByTypeNameAsync_Should_Return_NotFound_When_Type_Does_Not_Exist()
    {
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var name = "Unknown";
        mockRepo.GetCourseEventTypeByTypeNameAsync(name, Arg.Any<CancellationToken>())
            .Returns((CourseEventType?)null);

        var service = CreateService(mockRepo, out var mockCache);

        var result = await service.GetCourseEventTypeByTypeNameAsync(name, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Equal($"Course event type with name '{name}' not found.", result.ErrorMessage);
        await mockRepo.Received(1).GetCourseEventTypeByTypeNameAsync(name, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByTypeNameAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetCourseEventTypeByTypeNameAsync("Online", Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseEventType?>(new Exception("Database error")));

        var service = CreateService(mockRepo, out _);

        var result = await service.GetCourseEventTypeByTypeNameAsync("Online", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving the course event type", result.ErrorMessage);
    }

    #endregion

    #region GetCourseEventTypeByIdAsync Tests

    [Fact]
    public async Task GetCourseEventTypeByIdAsync_Should_Return_Type_When_Type_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var courseEventType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(courseEventType);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetCourseEventTypeByIdAsync(typeId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(typeId, result.Value.Id);
        Assert.Equal("Online", result.Value.Name);

        await mockRepo.Received(1).GetByIdAsync(typeId, Arg.Any<CancellationToken>());
        await mockCache.Received(1).GetByIdAsync(
            typeId,
            Arg.Any<Func<CancellationToken, Task<CourseEventType?>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByIdAsync_Should_Return_NotFound_When_Type_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns((CourseEventType)null!);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetCourseEventTypeByIdAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course event type with ID '{typeId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetCourseEventTypeByIdAsync_Should_Return_BadRequest_When_TypeId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetCourseEventTypeByIdAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByIdAsync_Should_Return_BadRequest_When_TypeId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetCourseEventTypeByIdAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCourseEventTypeByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseEventType?>(new Exception("Database error")));

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.GetCourseEventTypeByIdAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the course event type", result.ErrorMessage);
    }

    #endregion

    #region UpdateCourseEventTypeAsync Tests

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");
        var updatedType = CourseEventType.Reconstitute(typeId, "Virtual");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>())
            .Returns(updatedType);

        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(typeId, "Virtual");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(typeId, result.Value.Id);
        Assert.Equal("Virtual", result.Value.Name);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<int>(id => id == typeId),
            Arg.Is<CourseEventType>(t => t.Id == typeId && t.Name == "Virtual"),
            Arg.Any<CancellationToken>());
        mockCache.Received(1).ResetEntity(existingType);
        mockCache.Received(1).SetEntity(updatedType);
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.UpdateCourseEventTypeAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(0, "Online");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Course event type with ID '0' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeName_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CourseEventType.Reconstitute(1, "Online"));
        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(1, "");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Type name cannot be empty or whitespace.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_BadRequest_When_TypeName_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CourseEventType.Reconstitute(1, "Online"));
        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(1, "   ");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Type name cannot be empty or whitespace.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_NotFound_When_Type_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns((CourseEventType)null!);

        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(typeId, "Virtual");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Course event type with ID '{typeId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>());
        mockCache.DidNotReceive().ResetEntity(Arg.Any<CourseEventType>());
        mockCache.DidNotReceive().SetEntity(Arg.Any<CourseEventType>());
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<CourseEventType>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CourseEventType?>(new Exception("Database error")));

        var service = CreateService(mockRepo, out var mockCache);
        var input = new UpdateCourseEventTypeInput(typeId, "Virtual");

        // Act
        var result = await service.UpdateCourseEventTypeAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the course event type", result.ErrorMessage);
    }

    #endregion

    #region DeleteCourseEventTypeAsync Tests

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_Success_When_Type_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.IsInUseAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(typeId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(typeId, Arg.Any<CancellationToken>());
        mockCache.Received(1).ResetEntity(existingType);
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.IsInUseAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        mockCache.DidNotReceive().ResetEntity(Arg.Any<CourseEventType>());
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_BadRequest_When_TypeId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_BadRequest_When_TypeId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_NotFound_When_Type_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns((CourseEventType)null!);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"Course event type with ID '{typeId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        mockCache.DidNotReceive().ResetEntity(Arg.Any<CourseEventType>());
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_Conflict_When_Type_Is_In_Use()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.IsInUseAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);        Assert.Contains($"Cannot delete course event type with ID '{typeId}' because it is being used by one or more course events", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ICourseEventTypeRepository>();
        var typeId = 1;
        var existingType = CourseEventType.Reconstitute(typeId, "Online");

        mockRepo.GetByIdAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(existingType);

        mockRepo.RemoveAsync(typeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = CreateService(mockRepo, out var mockCache);

        // Act
        var result = await service.DeleteCourseEventTypeAsync(typeId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the course event type", result.ErrorMessage);
    }

    #endregion
}

