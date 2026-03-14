using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.Courses.Models;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Tests.Unit.Domain.Modules.Courses.Models;

public class CourseWithEvents_Tests
{
    [Fact]
    public void Constructor_Should_Create_CourseWithEvents_When_Parameters_Are_Valid()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>();

        // Act
        var courseWithEvents = new CourseWithEvents(course, events);

        // Assert
        Assert.NotNull(courseWithEvents);
        Assert.Equal(course, courseWithEvents.Course);
        Assert.Equal(events, courseWithEvents.Events);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Course_Is_Null()
    {
        // Arrange
        Course course = null!;
        var events = new List<CourseEvent>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CourseWithEvents(course, events));

        Assert.Equal("course", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Events_Is_Null()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        IReadOnlyList<CourseEvent> events = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CourseWithEvents(course, events));

        Assert.Equal("events", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_Accept_Empty_Events_List()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>();

        // Act
        var courseWithEvents = new CourseWithEvents(course, events);

        // Assert
        Assert.NotNull(courseWithEvents.Events);
        Assert.Empty(courseWithEvents.Events);
    }

    [Fact]
    public void Constructor_Should_Store_Events_List()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Reconstitute(courseId, "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>
        {
            CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow, 1000m, 20, 1, VenueType.Reconstitute(1, "InPerson")),
            CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow.AddDays(1), 1500m, 25, 1, VenueType.Reconstitute(1, "InPerson"))
        };

        // Act
        var courseWithEvents = new CourseWithEvents(course, events);

        // Assert
        Assert.Equal(2, courseWithEvents.Events.Count);
        Assert.Equal(events, courseWithEvents.Events);
    }

    [Fact]
    public void Properties_Should_Be_Initialized_Correctly()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Reconstitute(courseId, "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>
        {
            CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow, 1000m, 20, 1, VenueType.Reconstitute(1, "InPerson"))
        };

        // Act
        var courseWithEvents = new CourseWithEvents(course, events);

        // Assert
        Assert.Same(course, courseWithEvents.Course);
        Assert.Equal(courseId, courseWithEvents.Course.Id);
        Assert.Equal("Test Course", courseWithEvents.Course.Title);
        Assert.Single(courseWithEvents.Events);
    }

    [Fact]
    public void Course_Property_Should_Have_Init_Accessor()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>();

        // Act
        var courseWithEvents = new CourseWithEvents(course, events)
        {
            Course = course
        };

        // Assert
        Assert.Equal(course, courseWithEvents.Course);
    }

    [Fact]
    public void Events_Property_Should_Have_Init_Accessor()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>();

        // Act
        var courseWithEvents = new CourseWithEvents(course, events)
        {
            Events = events
        };

        // Assert
        Assert.Equal(events, courseWithEvents.Events);
    }

    [Fact]
    public void Constructor_Should_Handle_Large_Number_Of_Events()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Reconstitute(courseId, "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>();

        for (int i = 0; i < 100; i++)
        {
            events.Add(CourseEvent.Reconstitute(Guid.NewGuid(), courseId, DateTime.UtcNow.AddDays(i), 1000m + i, 20, 1, VenueType.Reconstitute(1, "InPerson")));
        }

        // Act
        var courseWithEvents = new CourseWithEvents(course, events);

        // Assert
        Assert.Equal(100, courseWithEvents.Events.Count);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentException_When_Event_Belongs_To_Another_Course()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events = new List<CourseEvent>
        {
            CourseEvent.Reconstitute(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000m, 20, 1, VenueType.Reconstitute(1, "InPerson"))
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CourseWithEvents(course, events));
        Assert.Equal("events", exception.ParamName);
    }

    [Fact]
    public void Two_Instances_With_Same_Course_Should_Have_Same_Course_Reference()
    {
        // Arrange
        var course = Course.Reconstitute(Guid.NewGuid(), "Test Course", "Test Description", 10);
        var events1 = new List<CourseEvent>();
        var events2 = new List<CourseEvent>();

        // Act
        var courseWithEvents1 = new CourseWithEvents(course, events1);
        var courseWithEvents2 = new CourseWithEvents(course, events2);

        // Assert
        Assert.Same(courseWithEvents1.Course, courseWithEvents2.Course);
    }
}
