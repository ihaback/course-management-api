using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.Courses.Models;

public sealed class Course
{
    public Guid Id { get; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int DurationInDays { get; private set; }

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private Course(
        Guid id,
        string title,
        string description,
        int durationInDays)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Course id cannot be empty.", nameof(id));

        Id = id;
        SetValues(title, description, durationInDays);
    }

    public static Course Create(string title, string description, int durationInDays)
        => new(Guid.NewGuid(), title, description, durationInDays);

    public static Course Reconstitute(Guid id, string title, string description, int durationInDays)
        => new(id, title, description, durationInDays);

    public void Update(string title, string description, int durationInDays)
    {
        SetValues(title, description, durationInDays);
    }

    private void SetValues(string title, string description, int durationInDays)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title cannot be empty or whitespace.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Course description cannot be empty or whitespace.", nameof(description));

        if (durationInDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationInDays), "Course duration must be greater than zero.");

        Title = title.Trim();
        Description = description.Trim();
        DurationInDays = durationInDays;
    }
}
