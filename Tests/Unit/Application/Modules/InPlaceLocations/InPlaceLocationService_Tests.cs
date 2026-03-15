using Backend.Application.Common;
using Backend.Application.Modules.InPlaceLocations;
using Backend.Application.Modules.InPlaceLocations.Inputs;
using Backend.Domain.Modules.InPlaceLocations.Contracts;
using Backend.Domain.Modules.InPlaceLocations.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.InPlaceLocations;

public class InPlaceLocationService_Tests
{
    #region CreateInPlaceLocationAsync Tests

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var expectedInPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        mockRepo.AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(expectedInPlaceLocation);

        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, 101, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.LocationId);
        Assert.Equal(101, result.Value.RoomNumber);
        Assert.Equal(30, result.Value.Seats);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<InPlaceLocation>(ipl => ipl.LocationId == 1 && ipl.RoomNumber == 101 && ipl.Seats == 30),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.CreateInPlaceLocationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(0, 101, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(-1, 101, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_RoomNumber_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, 0, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_RoomNumber_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, -1, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_Seats_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, 101, 0);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_BadRequest_When_Seats_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, 101, -1);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInPlaceLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InPlaceLocation>(new Exception("Database error")));

        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(1, 101, 30);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the in-place location", result.ErrorMessage);
    }

    [Theory]
    [InlineData(1, 101, 30)]
    [InlineData(1, 102, 25)]
    [InlineData(2, 201, 50)]
    public async Task CreateInPlaceLocationAsync_Should_Create_InPlaceLocation_With_Various_Valid_Inputs(
        int locationId, int roomNumber, int seats)
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var expectedInPlaceLocation = InPlaceLocation.Reconstitute(1, locationId, roomNumber, seats);

        mockRepo.AddAsync(Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(expectedInPlaceLocation);

        var service = new InPlaceLocationService(mockRepo);
        var input = new CreateInPlaceLocationInput(locationId, roomNumber, seats);

        // Act
        var result = await service.CreateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(locationId, result.Value.LocationId);
        Assert.Equal(roomNumber, result.Value.RoomNumber);
        Assert.Equal(seats, result.Value.Seats);
    }

    [Fact]
    public void InPlaceLocationService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InPlaceLocationService(null!));
    }

    #endregion

    #region GetAllInPlaceLocationsAsync Tests

    [Fact]
    public async Task GetAllInPlaceLocationsAsync_Should_Return_All_InPlaceLocations_When_InPlaceLocations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocations = new List<InPlaceLocation>
        {
            InPlaceLocation.Reconstitute(1, 1, 101, 30),
            InPlaceLocation.Reconstitute(2, 1, 102, 25),
            InPlaceLocation.Reconstitute(3, 2, 201, 50)
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(inPlaceLocations);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetAllInPlaceLocationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllInPlaceLocationsAsync_Should_Return_Empty_List_When_No_InPlaceLocations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<InPlaceLocation>());

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetAllInPlaceLocationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllInPlaceLocationsAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<InPlaceLocation>>(new Exception("Database connection failed")));

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetAllInPlaceLocationsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving in-place locations", result.ErrorMessage);
    }

    #endregion

    #region GetInPlaceLocationByIdAsync Tests

    [Fact]
    public async Task GetInPlaceLocationByIdAsync_Should_Return_InPlaceLocation_When_InPlaceLocation_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var inPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(inPlaceLocation);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationByIdAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(inPlaceLocationId, result.Value.Id);
        Assert.Equal(101, result.Value.RoomNumber);

        await mockRepo.Received(1).GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationByIdAsync_Should_Return_NotFound_When_InPlaceLocation_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns((InPlaceLocation)null!);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationByIdAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"In-place location with ID '{inPlaceLocationId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetInPlaceLocationByIdAsync_Should_Return_BadRequest_When_InPlaceLocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationByIdAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationByIdAsync_Should_Return_BadRequest_When_InPlaceLocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationByIdAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InPlaceLocation?>(new Exception("Database error")));

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationByIdAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the in-place location", result.ErrorMessage);
    }

    #endregion

    #region GetInPlaceLocationsByLocationIdAsync Tests

    [Fact]
    public async Task GetInPlaceLocationsByLocationIdAsync_Should_Return_InPlaceLocations_When_InPlaceLocations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var locationId = 1;
        var inPlaceLocations = new List<InPlaceLocation>
        {
            InPlaceLocation.Reconstitute(1, locationId, 101, 30),
            InPlaceLocation.Reconstitute(2, locationId, 102, 25)
        };

        mockRepo.GetInPlaceLocationsByLocationIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(inPlaceLocations);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationsByLocationIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());

        await mockRepo.Received(1).GetInPlaceLocationsByLocationIdAsync(locationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationsByLocationIdAsync_Should_Return_Empty_List_When_No_InPlaceLocations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var locationId = 1;

        mockRepo.GetInPlaceLocationsByLocationIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(new List<InPlaceLocation>());

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationsByLocationIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetInPlaceLocationsByLocationIdAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationsByLocationIdAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetInPlaceLocationsByLocationIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationsByLocationIdAsync_Should_Return_BadRequest_When_LocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationsByLocationIdAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetInPlaceLocationsByLocationIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInPlaceLocationsByLocationIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var locationId = 1;

        mockRepo.GetInPlaceLocationsByLocationIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<InPlaceLocation>>(new Exception("Database error")));

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.GetInPlaceLocationsByLocationIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving in-place locations", result.ErrorMessage);
    }

    #endregion

    #region UpdateInPlaceLocationAsync Tests

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);
        var updatedInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 35);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(updatedInPlaceLocation);

        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(inPlaceLocationId, 1, 101, 35);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(35, result.Value.Seats);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<int>(id => id == inPlaceLocationId),
            Arg.Is<InPlaceLocation>(ipl => ipl.Id == inPlaceLocationId && ipl.Seats == 35),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_BadRequest_When_InPlaceLocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(0, 1, 101, 30);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(InPlaceLocation.Reconstitute(1, 1, 101, 30));
        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(1, 0, 101, 30);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_BadRequest_When_RoomNumber_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(InPlaceLocation.Reconstitute(1, 1, 101, 30));
        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(1, 1, 0, 30);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_BadRequest_When_Seats_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(InPlaceLocation.Reconstitute(1, 1, 101, 30));
        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(1, 1, 101, 0);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_NotFound_When_InPlaceLocation_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns((InPlaceLocation)null!);

        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(inPlaceLocationId, 1, 101, 30);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"In-place location with ID '{inPlaceLocationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InPlaceLocation?>(new Exception("Database error")));

        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(inPlaceLocationId, 1, 101, 35);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the in-place location", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_Should_Return_Conflict_When_Repository_Throws_DbUpdateException()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<InPlaceLocation>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InPlaceLocation?>(new DbUpdateException("FK violation")));

        var service = new InPlaceLocationService(mockRepo);
        var input = new UpdateInPlaceLocationInput(inPlaceLocationId, 9999, 101, 35);

        // Act
        var result = await service.UpdateInPlaceLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("requested location reference is invalid", result.ErrorMessage);
    }

    #endregion

    #region DeleteInPlaceLocationAsync Tests

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_Success_When_InPlaceLocation_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.HasCourseEventsAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(inPlaceLocationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_BadRequest_When_InPlaceLocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_BadRequest_When_InPlaceLocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);        Assert.Contains("greater than zero", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_NotFound_When_InPlaceLocation_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns((InPlaceLocation)null!);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"In-place location with ID '{inPlaceLocationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_Conflict_When_InPlaceLocation_Has_CourseEvents()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.HasCourseEventsAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);        Assert.Contains("Cannot delete in-place location", result.ErrorMessage);
        Assert.Contains("assigned to course events", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.HasCourseEventsAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the in-place location", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<IInPlaceLocationRepository>();
        var inPlaceLocationId = 1;
        var existingInPlaceLocation = InPlaceLocation.Reconstitute(inPlaceLocationId, 1, 101, 30);

        mockRepo.GetByIdAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(existingInPlaceLocation);

        mockRepo.HasCourseEventsAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(inPlaceLocationId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = new InPlaceLocationService(mockRepo);

        // Act
        var result = await service.DeleteInPlaceLocationAsync(inPlaceLocationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);    }

    #endregion

    private sealed class DbUpdateException(string message) : Exception(message);
}

