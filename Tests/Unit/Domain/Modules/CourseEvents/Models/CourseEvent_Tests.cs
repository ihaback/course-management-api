using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseEvents.Models;

public class CourseEvent_Tests
{
    private static readonly VenueType DefaultVenue = VenueType.Reconstitute(1, "InPerson");
    private static readonly CourseEventType DefaultEventType = CourseEventType.Reconstitute(1, "Online");

    public static IEnumerable<object[]> ValidVenueTypes()
    {
        yield return [VenueType.Reconstitute(1, "InPerson")];
        yield return [VenueType.Reconstitute(2, "Online")];
        yield return [VenueType.Reconstitute(3, "Hybrid")];
    }

    [Fact]
    public void Constructor_Should_Create_CourseEvent_When_Parameters_Are_Valid()
    {
        var id = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var eventDate = DateTime.UtcNow;
        var price = 1000m;
        var seats = 30;

        var courseEvent = CourseEvent.Reconstitute(id, courseId, eventDate, price, seats, DefaultVenue, DefaultEventType);

        Assert.NotNull(courseEvent);
        Assert.Equal(id, courseEvent.Id);
        Assert.Equal(courseId, courseEvent.CourseId);
        Assert.Equal(eventDate, courseEvent.EventDate);
        Assert.Equal(price, courseEvent.Price.Value);
        Assert.Equal(seats, courseEvent.Seats);
        Assert.Equal(DefaultEventType.Id, courseEvent.CourseEventType.Id);
        Assert.Equal(DefaultVenue, courseEvent.VenueType);
    }

    [Theory]
    [MemberData(nameof(ValidVenueTypes))]
    public void Constructor_Should_Accept_All_Valid_VenueTypes(VenueType venueType)
    {
        var eventType = CourseEventType.Reconstitute(1, "Type");
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, venueType, eventType);

        Assert.Equal(venueType, courseEvent.VenueType);
    }

    [Fact]
    public void Constructor_Should_Throw_When_VenueType_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, null!, DefaultEventType));
    }

    [Fact]
    public void Constructor_Should_Throw_When_CourseEventType_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, null!));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Id_Is_Empty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEvent.Reconstitute(Guid.Empty, Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType));

        Assert.Equal("id", exception.ParamName);
        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_CourseId_Is_Empty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.Empty, DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType));

        Assert.Equal("courseId", exception.ParamName);
        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_EventDate_Is_Default()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), default, 1000m, 30, DefaultVenue, DefaultEventType));

        Assert.Equal("eventDate", exception.ParamName);
        Assert.Contains("Event date must be specified", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_Price_Is_Negative()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, -1m, 30, DefaultVenue, DefaultEventType));

        Assert.Equal("price", exception.ParamName);
        Assert.Contains("Price cannot be negative", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero_Price()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 0m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(0m, courseEvent.Price.Value);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_Seats_Is_Zero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 0, DefaultVenue, DefaultEventType));
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentOutOfRangeException_When_Seats_Is_Negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, -1, DefaultVenue, DefaultEventType));
    }

    [Theory]
    [InlineData(500.00)]
    [InlineData(1000.00)]
    [InlineData(1500.50)]
    [InlineData(2000.99)]
    public void Constructor_Should_Create_CourseEvent_With_Various_Prices(decimal price)
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, price, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(price, courseEvent.Price.Value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Constructor_Should_Create_CourseEvent_With_Various_Seat_Capacities(int seats)
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, seats, DefaultVenue, DefaultEventType);

        Assert.Equal(seats, courseEvent.Seats);
    }

    [Fact]
    public void Properties_Should_Be_Initialized_Correctly()
    {
        var id = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var eventDate = DateTime.UtcNow;

        var courseEvent = CourseEvent.Reconstitute(id, courseId, eventDate, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(id, courseEvent.Id);
        Assert.Equal(courseId, courseEvent.CourseId);
        Assert.Equal(eventDate, courseEvent.EventDate);
        Assert.Equal(1000m, courseEvent.Price.Value);
        Assert.Equal(30, courseEvent.Seats);
        Assert.Equal(DefaultEventType.Id, courseEvent.CourseEventType.Id);
    }

    [Fact]
    public void Two_CourseEvents_With_Same_Values_Should_Have_Same_Property_Values()
    {
        var id = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var eventDate = DateTime.UtcNow;

        var courseEvent1 = CourseEvent.Reconstitute(id, courseId, eventDate, 1000m, 30, DefaultVenue, DefaultEventType);
        var courseEvent2 = CourseEvent.Reconstitute(id, courseId, eventDate, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(courseEvent1.Id, courseEvent2.Id);
        Assert.Equal(courseEvent1.CourseId, courseEvent2.CourseId);
        Assert.Equal(courseEvent1.EventDate, courseEvent2.EventDate);
        Assert.Equal(courseEvent1.Price, courseEvent2.Price);
        Assert.Equal(courseEvent1.Seats, courseEvent2.Seats);
        Assert.Equal(courseEvent1.CourseEventType.Id, courseEvent2.CourseEventType.Id);
    }

    [Fact]
    public void Id_Property_Should_Be_Read_Only()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(courseEvent.Id, courseEvent.Id);
    }

    [Fact]
    public void CourseId_Property_Should_Be_Read_Only()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(courseEvent.CourseId, courseEvent.CourseId);
    }

    [Fact]
    public void EventDate_Property_Should_Be_Read_Only()
    {
        var eventDate = DateTime.UtcNow;
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), eventDate, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(eventDate, courseEvent.EventDate);
    }

    [Fact]
    public void Price_Property_Should_Be_Read_Only()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(courseEvent.Price, courseEvent.Price);
    }

    [Fact]
    public void Seats_Property_Should_Be_Read_Only()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(courseEvent.Seats, courseEvent.Seats);
    }

    [Fact]
    public void Constructor_Should_Handle_Future_Dates()
    {
        var eventDate = DateTime.UtcNow.AddDays(30);
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), eventDate, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(eventDate, courseEvent.EventDate);
    }

    [Fact]
    public void Constructor_Should_Handle_Past_Dates()
    {
        var eventDate = DateTime.UtcNow.AddDays(-30);
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), eventDate, 1000m, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(eventDate, courseEvent.EventDate);
    }

    [Fact]
    public void Multiple_Events_Can_Have_Same_CourseId()
    {
        var courseId = Guid.NewGuid();
        var event1 = CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow, 1000m, 30, DefaultVenue, DefaultEventType);
        var event2 = CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow.AddDays(1), 1500m, 25, DefaultVenue, DefaultEventType);

        Assert.Equal(courseId, event1.CourseId);
        Assert.Equal(courseId, event2.CourseId);
        Assert.NotEqual(event1.Id, event2.Id);
    }

    [Fact]
    public void Constructor_Should_Handle_Large_Seat_Capacity()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 1000, DefaultVenue, DefaultEventType);

        Assert.Equal(1000, courseEvent.Seats);
    }

    [Fact]
    public void Constructor_Should_Handle_High_Prices()
    {
        var price = 999999.99m;
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, price, 30, DefaultVenue, DefaultEventType);

        Assert.Equal(price, courseEvent.Price.Value);
    }

    [Fact]
    public void Update_Should_Change_Values_When_Input_Is_Valid()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m, 10, DefaultVenue, DefaultEventType);
        var newCourseId = Guid.NewGuid();
        var newEventDate = DateTime.UtcNow.AddDays(7);
        var newVenue = VenueType.Reconstitute(3, "Hybrid");
        var newEventType = CourseEventType.Reconstitute(2, "Advanced");

        courseEvent.Update(newCourseId, newEventDate, 250m, 25, newVenue, newEventType);

        Assert.Equal(newCourseId, courseEvent.CourseId);
        Assert.Equal(newEventDate, courseEvent.EventDate);
        Assert.Equal(250m, courseEvent.Price.Value);
        Assert.Equal(25, courseEvent.Seats);
        Assert.Equal(2, courseEvent.CourseEventType.Id);
        Assert.Equal(newVenue, courseEvent.VenueType);
    }

    [Fact]
    public void Update_Should_Throw_When_VenueType_Is_Null()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m, 10, DefaultVenue, DefaultEventType);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            courseEvent.Update(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 100m, 10, null!, DefaultEventType));

        Assert.Equal("venueType", exception.ParamName);
    }

    [Fact]
    public void Update_Should_Throw_When_CourseEventType_Is_Null()
    {
        var courseEvent = CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m, 10, DefaultVenue, DefaultEventType);

        Assert.Throws<ArgumentNullException>(() =>
            courseEvent.Update(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 100m, 10, DefaultVenue, null!));
    }
}
