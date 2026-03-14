using Backend.Application.Common;
using Backend.Application.Modules.Locations;
using Backend.Application.Modules.Locations.Inputs;
using Backend.Domain.Modules.Locations.Contracts;
using Backend.Domain.Modules.Locations.Models;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.Locations;

public class LocationService_Tests
{
    #region CreateLocationAsync Tests

    [Fact]
    public async Task CreateLocationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var expectedLocation = Location.Reconstitute(1, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>())
            .Returns(expectedLocation);

        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "11143", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Kungsgatan 12", result.Value.StreetName);
        Assert.Equal("11143", result.Value.PostalCode);
        Assert.Equal("Stockholm", result.Value.City);

        await mockRepo.Received(1).AddAsync(
            Arg.Is<Location>(l => l.StreetName == "Kungsgatan 12" && l.PostalCode == "11143" && l.City == "Stockholm"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.CreateLocationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_StreetName_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("", "11143", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_StreetName_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("   ", "11143", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_PostalCode_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_PostalCode_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "   ", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_City_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "11143", "");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_City_Is_Whitespace()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "11143", "   ");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Location>(new Exception("Database error")));

        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "11143", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while creating the location", result.ErrorMessage);
            }

    [Theory]
    [InlineData("Drottninggatan 1", "11121", "Stockholm")]
    [InlineData("Storgatan 5", "41138", "Göteborg")]
    [InlineData("Vasagatan 10", "21120", "Malmö")]
    public async Task CreateLocationAsync_Should_Create_Location_With_Various_Valid_Inputs(
        string streetName, string postalCode, string city)
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var expectedLocation = Location.Reconstitute(1, streetName, postalCode, city);

        mockRepo.AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>())
            .Returns(expectedLocation);

        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput(streetName, postalCode, city);

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(streetName, result.Value.StreetName);
        Assert.Equal(postalCode, result.Value.PostalCode);
        Assert.Equal(city, result.Value.City);
    }

    [Fact]
    public async Task CreateLocationAsync_Should_Return_BadRequest_When_PostalCode_Has_Invalid_Format()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new CreateLocationInput("Kungsgatan 12", "12 345", "Stockholm");

        // Act
        var result = await service.CreateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Postal code must consist of exactly 5 digits with no spaces", result.ErrorMessage);

        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void LocationService_Constructor_Should_Throw_When_Repository_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LocationService(null!));
    }

    #endregion

    #region GetAllLocationsAsync Tests

    [Fact]
    public async Task GetAllLocationsAsync_Should_Return_All_Locations_When_Locations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locations = new List<Location>
        {
            Location.Reconstitute(1, "Kungsgatan 12", "11143", "Stockholm"),
            Location.Reconstitute(2, "Storgatan 5", "41138", "Göteborg"),
            Location.Reconstitute(3, "Vasagatan 10", "21120", "Malmö")
        };

        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(locations);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetAllLocationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());

        await mockRepo.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllLocationsAsync_Should_Return_Empty_List_When_No_Locations_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Location>());

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetAllLocationsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllLocationsAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<Location>>(new Exception("Database connection failed")));

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetAllLocationsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Contains("An error occurred while retrieving locations", result.ErrorMessage);
            }

    #endregion

    #region GetLocationByIdAsync Tests

    [Fact]
    public async Task GetLocationByIdAsync_Should_Return_Location_When_Location_Exists()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var location = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(location);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetLocationByIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(locationId, result.Value.Id);
        Assert.Equal("Kungsgatan 12", result.Value.StreetName);

        await mockRepo.Received(1).GetByIdAsync(locationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLocationByIdAsync_Should_Return_NotFound_When_Location_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns((Location)null!);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetLocationByIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Location with ID '{locationId}' not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetLocationByIdAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetLocationByIdAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLocationByIdAsync_Should_Return_BadRequest_When_LocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetLocationByIdAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLocationByIdAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Location?>(new Exception("Database error")));

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.GetLocationByIdAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while retrieving the location", result.ErrorMessage);
            }

    #endregion

    #region UpdateLocationAsync Tests

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_Success_When_Valid_Input()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");
        var updatedLocation = Location.Reconstitute(locationId, "Kungsgatan 15", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<Location>(), Arg.Any<CancellationToken>())
            .Returns(updatedLocation);

        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(locationId, "Kungsgatan 15", "11143", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal("Kungsgatan 15", result.Value.StreetName);

        await mockRepo.Received(1).UpdateAsync(
            Arg.Is<int>(id => id == locationId),
            Arg.Is<Location>(l => l.Id == locationId && l.StreetName == "Kungsgatan 15"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_Input_Is_Null()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.UpdateLocationAsync(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(0, "Kungsgatan 12", "11143", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);

        await mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_StreetName_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Location.Reconstitute(1, "Old Street", "11111", "City"));
        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(1, "", "11143", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_PostalCode_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Location.Reconstitute(1, "Old Street", "11111", "City"));
        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(1, "Kungsgatan 12", "", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_City_Is_Empty()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Location.Reconstitute(1, "Old Street", "11111", "City"));
        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(1, "Kungsgatan 12", "11143", "");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("cannot be empty or whitespace", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_NotFound_When_Location_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns((Location)null!);

        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(locationId, "Kungsgatan 12", "11143", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains($"Location with ID '{locationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.UpdateAsync(Arg.Any<int>(), Arg.Any<Location>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Location?>(new Exception("Database error")));

        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(locationId, "Kungsgatan 15", "11143", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("An error occurred while updating the location", result.ErrorMessage);
            }

    [Fact]
    public async Task UpdateLocationAsync_Should_Return_BadRequest_When_PostalCode_Has_Invalid_Format()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        mockRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Location.Reconstitute(1, "Old Street", "11111", "City"));

        var service = new LocationService(mockRepo);
        var input = new UpdateLocationInput(1, "Kungsgatan 12", "123 45", "Stockholm");

        // Act
        var result = await service.UpdateLocationAsync(input, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        Assert.Null(result.Value);
        Assert.Contains("Postal code must consist of exactly 5 digits with no spaces", result.ErrorMessage);

        await mockRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Location>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteLocationAsync Tests

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_Success_When_Location_Is_Deleted()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.HasInPlaceLocationsAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(locationId, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        await mockRepo.Received(1).RemoveAsync(locationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Zero()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(0, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_BadRequest_When_LocationId_Is_Negative()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(-1, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.BadRequest, result.ErrorType);
        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_NotFound_When_Location_Does_Not_Exist()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns((Location)null!);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.NotFound, result.ErrorType);        Assert.Contains($"Location with ID '{locationId}' not found", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_Conflict_When_Location_Has_InPlaceLocations()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.HasInPlaceLocationsAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(true);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Conflict, result.ErrorType);        Assert.Contains("Cannot delete location", result.ErrorMessage);
        Assert.Contains("has in-place locations", result.ErrorMessage);

        await mockRepo.DidNotReceive().RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_InternalServerError_When_Repository_Throws_Exception()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.HasInPlaceLocationsAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Database error")));

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);        Assert.Contains("An error occurred while deleting the location", result.ErrorMessage);
            }

    [Fact]
    public async Task DeleteLocationAsync_Should_Return_InternalServerError_When_Delete_Returns_False()
    {
        // Arrange
        var mockRepo = Substitute.For<ILocationRepository>();
        var locationId = 1;
        var existingLocation = Location.Reconstitute(locationId, "Kungsgatan 12", "11143", "Stockholm");

        mockRepo.GetByIdAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(existingLocation);

        mockRepo.HasInPlaceLocationsAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(false);

        mockRepo.RemoveAsync(locationId, Arg.Any<CancellationToken>())
            .Returns(false);

        var service = new LocationService(mockRepo);

        // Act
        var result = await service.DeleteLocationAsync(locationId, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ErrorTypes.Error, result.ErrorType);    }

    #endregion
}

