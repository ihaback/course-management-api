using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.CourseRegistrationStatuses.Models;

public sealed class CourseRegistrationStatus
{
    public int Id { get; }
    public string Name { get; private set; } = null!;

    public static CourseRegistrationStatus Pending { get; } = Reconstitute(0, "Pending");
    public static CourseRegistrationStatus Paid { get; } = Reconstitute(1, "Paid");
    public static CourseRegistrationStatus Cancelled { get; } = Reconstitute(2, "Cancelled");
    public static CourseRegistrationStatus Refunded { get; } = Reconstitute(3, "Refunded");

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private CourseRegistrationStatus(int id, string name)
    {
        if (id < 0)
            throw new ArgumentException("Id must be zero or positive.", nameof(id));

        Id = id;
        SetValues(name);
    }

    public static CourseRegistrationStatus Create(string name)
        => new(0, name);

    public static CourseRegistrationStatus Reconstitute(int id, string name)
        => new(id, name);

    public void Update(string name)
    {
        SetValues(name);
    }

    private void SetValues(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

        Name = name.Trim();
    }
}
