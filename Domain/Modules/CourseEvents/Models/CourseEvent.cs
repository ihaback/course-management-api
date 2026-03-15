using Backend.Domain.Common.ValueObjects;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.CourseEvents.Models;

public sealed class CourseEvent
{
    public Guid Id { get; }
    public Guid CourseId { get; private set; }
    public DateTime EventDate { get; private set; }
    public Price Price { get; private set; } = null!;
    public int Seats { get; private set; }
    public CourseEventType CourseEventType { get; private set; }
    public VenueType VenueType { get; private set; }

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private CourseEvent(
        Guid id,
        Guid courseId,
        DateTime eventDate,
        Price price,
        int seats,
        VenueType venueType,
        CourseEventType? courseEventType = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("CourseEvent id cannot be empty.", nameof(id));

        Id = id;
        SetValues(courseId, eventDate, price, seats, venueType, courseEventType);
    }

    public static CourseEvent Create(
        Guid courseId,
        DateTime eventDate,
        decimal price,
        int seats,
        VenueType venueType,
        CourseEventType courseEventType)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        return new(Guid.NewGuid(), courseId, eventDate, Price.Create(price), seats, venueType, courseEventType);
    }

    public static CourseEvent Reconstitute(
        Guid id,
        Guid courseId,
        DateTime eventDate,
        decimal price,
        int seats,
        VenueType venueType,
        CourseEventType courseEventType)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        return new(id, courseId, eventDate, Price.Create(price), seats, venueType, courseEventType);
    }

    public void Update(
        Guid courseId,
        DateTime eventDate,
        decimal price,
        int seats,
        VenueType venueType,
        CourseEventType courseEventType)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        SetValues(courseId, eventDate, Price.Create(price), seats, venueType, courseEventType);
    }

    [MemberNotNull(nameof(CourseEventType), nameof(VenueType))]
    private void SetValues(
        Guid courseId,
        DateTime eventDate,
        Price price,
        int seats,
        VenueType venueType,
        CourseEventType? courseEventType)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("Course id cannot be empty.", nameof(courseId));

        if (eventDate == default)
            throw new ArgumentException("Event date must be specified.", nameof(eventDate));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seats);
        ArgumentNullException.ThrowIfNull(venueType);
        ArgumentNullException.ThrowIfNull(courseEventType);

        CourseId = courseId;
        EventDate = eventDate;
        Price = price;
        Seats = seats;
        CourseEventType = courseEventType;
        VenueType = venueType;
    }
}
