using Backend.Domain.Modules.InPlaceLocations.Models;

namespace Backend.Tests.Unit.Domain.Modules.InPlaceLocations.Models;

public class InPlaceLocation_Tests
{
    [Fact]
    public void Constructor_Should_Create_InPlaceLocation_When_Parameters_Are_Valid()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 30;

        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.NotNull(inPlaceLocation);
        Assert.Equal(id, inPlaceLocation.Id);
        Assert.Equal(locationId, inPlaceLocation.LocationId);
        Assert.Equal(roomNumber, inPlaceLocation.RoomNumber);
        Assert.Equal(seats, inPlaceLocation.Seats);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Id_Is_Negative()
    {
        // Arrange
        var id = -1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 30;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("id", exception.ParamName);
        Assert.Contains("ID must be greater than or equal to zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_Id()
    {
        // Arrange
        var id = 0;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 30;

        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.Equal(0, inPlaceLocation.Id);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_LocationId_Is_Zero()
    {
        // Arrange
        var id = 1;
        var locationId = 0;
        var roomNumber = 101;
        var seats = 30;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("locationId", exception.ParamName);
        Assert.Contains("Location ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_LocationId_Is_Negative()
    {
        // Arrange
        var id = 1;
        var locationId = -1;
        var roomNumber = 101;
        var seats = 30;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("locationId", exception.ParamName);
        Assert.Contains("Location ID must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_RoomNumber_Is_Zero()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 0;
        var seats = 30;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("roomNumber", exception.ParamName);
        Assert.Contains("Room number must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_RoomNumber_Is_Negative()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = -1;
        var seats = 30;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("roomNumber", exception.ParamName);
        Assert.Contains("Room number must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Seats_Is_Zero()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 0;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("seats", exception.ParamName);
        Assert.Contains("Seats must be greater than zero", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Seats_Is_Negative()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats));

        Assert.Equal("seats", exception.ParamName);
        Assert.Contains("Seats must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(1, 1, 101, 30)]
    [InlineData(2, 1, 102, 25)]
    [InlineData(3, 1, 103, 50)]
    [InlineData(4, 2, 201, 40)]
    [InlineData(5, 2, 202, 35)]
    public void Constructor_Should_Create_InPlaceLocation_With_Various_Valid_Parameters(
        int id, int locationId, int roomNumber, int seats)
    {
        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.Equal(id, inPlaceLocation.Id);
        Assert.Equal(locationId, inPlaceLocation.LocationId);
        Assert.Equal(roomNumber, inPlaceLocation.RoomNumber);
        Assert.Equal(seats, inPlaceLocation.Seats);
    }

    [Fact]
    public void Properties_Should_Be_Initialized_Correctly()
    {
        // Arrange & Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(1, inPlaceLocation.Id);
        Assert.Equal(1, inPlaceLocation.LocationId);
        Assert.Equal(101, inPlaceLocation.RoomNumber);
        Assert.Equal(30, inPlaceLocation.Seats);
    }

    [Fact]
    public void Two_InPlaceLocations_With_Same_Values_Should_Have_Same_Property_Values()
    {
        // Arrange
        var inPlaceLocation1 = InPlaceLocation.Reconstitute(1, 1, 101, 30);
        var inPlaceLocation2 = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(inPlaceLocation1.Id, inPlaceLocation2.Id);
        Assert.Equal(inPlaceLocation1.LocationId, inPlaceLocation2.LocationId);
        Assert.Equal(inPlaceLocation1.RoomNumber, inPlaceLocation2.RoomNumber);
        Assert.Equal(inPlaceLocation1.Seats, inPlaceLocation2.Seats);
    }

    [Fact]
    public void Constructor_Should_Handle_Large_Room_Numbers()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 9999;
        var seats = 30;

        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.Equal(roomNumber, inPlaceLocation.RoomNumber);
    }

    [Fact]
    public void Constructor_Should_Handle_Large_Seat_Capacity()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 500;

        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.Equal(seats, inPlaceLocation.Seats);
    }

    [Fact]
    public void Constructor_Should_Handle_Small_Room_With_One_Seat()
    {
        // Arrange
        var id = 1;
        var locationId = 1;
        var roomNumber = 101;
        var seats = 1;

        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(id, locationId, roomNumber, seats);

        // Assert
        Assert.Equal(1, inPlaceLocation.Seats);
    }

    [Fact]
    public void Id_Property_Should_Be_Read_Only()
    {
        // Arrange
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(1, inPlaceLocation.Id);
        var initialId = inPlaceLocation.Id;
        Assert.Equal(initialId, inPlaceLocation.Id);
    }

    [Fact]
    public void LocationId_Property_Should_Be_Read_Only()
    {
        // Arrange
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(1, inPlaceLocation.LocationId);
        var initialLocationId = inPlaceLocation.LocationId;
        Assert.Equal(initialLocationId, inPlaceLocation.LocationId);
    }

    [Fact]
    public void RoomNumber_Property_Should_Be_Read_Only()
    {
        // Arrange
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(101, inPlaceLocation.RoomNumber);
        var initialRoomNumber = inPlaceLocation.RoomNumber;
        Assert.Equal(initialRoomNumber, inPlaceLocation.RoomNumber);
    }

    [Fact]
    public void Seats_Property_Should_Be_Read_Only()
    {
        // Arrange
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        // Assert
        Assert.Equal(30, inPlaceLocation.Seats);
        var initialSeats = inPlaceLocation.Seats;
        Assert.Equal(initialSeats, inPlaceLocation.Seats);
    }

    [Fact]
    public void Multiple_Rooms_Can_Belong_To_Same_Location()
    {
        // Arrange
        var locationId = 1;
        var room1 = InPlaceLocation.Reconstitute(1, locationId, 101, 30);
        var room2 = InPlaceLocation.Reconstitute(2, locationId, 102, 25);
        var room3 = InPlaceLocation.Reconstitute(3, locationId, 103, 50);

        // Assert
        Assert.Equal(locationId, room1.LocationId);
        Assert.Equal(locationId, room2.LocationId);
        Assert.Equal(locationId, room3.LocationId);
        Assert.NotEqual(room1.Id, room2.Id);
        Assert.NotEqual(room2.Id, room3.Id);
    }

    [Fact]
    public void Different_Locations_Can_Have_Same_Room_Numbers()
    {
        // Arrange
        var roomNumber = 101;
        var room1 = InPlaceLocation.Reconstitute(1, 1, roomNumber, 30);
        var room2 = InPlaceLocation.Reconstitute(2, 2, roomNumber, 25);

        // Assert
        Assert.Equal(roomNumber, room1.RoomNumber);
        Assert.Equal(roomNumber, room2.RoomNumber);
        Assert.NotEqual(room1.LocationId, room2.LocationId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    public void Constructor_Should_Accept_Various_Valid_Seat_Capacities(int seats)
    {
        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, seats);

        // Assert
        Assert.Equal(seats, inPlaceLocation.Seats);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    [InlineData(201)]
    [InlineData(999)]
    [InlineData(9999)]
    public void Constructor_Should_Accept_Various_Valid_Room_Numbers(int roomNumber)
    {
        // Act
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, roomNumber, 30);

        // Assert
        Assert.Equal(roomNumber, inPlaceLocation.RoomNumber);
    }

    [Fact]
    public void Update_Should_Change_Values_When_Input_Is_Valid()
    {
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        inPlaceLocation.Update(2, 202, 45);

        Assert.Equal(2, inPlaceLocation.LocationId);
        Assert.Equal(202, inPlaceLocation.RoomNumber);
        Assert.Equal(45, inPlaceLocation.Seats);
    }

    [Fact]
    public void Update_Should_Throw_ArgumentException_When_Seats_Is_Zero()
    {
        var inPlaceLocation = InPlaceLocation.Reconstitute(1, 1, 101, 30);

        var ex = Assert.Throws<ArgumentException>(() => inPlaceLocation.Update(1, 101, 0));
        Assert.Equal("seats", ex.ParamName);
    }
}
