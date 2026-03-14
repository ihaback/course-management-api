using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.CourseEventTypes.Models;

public sealed class CourseEventType
{
    public int Id { get; }
    public string Name { get; private set; } = null!;

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private CourseEventType(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(id);

        Id = id;
        SetValues(name);
    }

    public static CourseEventType Create(string name)
        => new(0, name);

    public static CourseEventType Reconstitute(int id, string name)
        => new(id, name);

    public void Update(string name)
    {
        SetValues(name);
    }

    private void SetValues(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Type name cannot be empty or whitespace.", nameof(name));

        Name = name.Trim();
    }
}
