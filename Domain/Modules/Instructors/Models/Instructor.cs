using Backend.Domain.Modules.InstructorRoles.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Backend.Domain.Modules.Instructors.Models;

public sealed class Instructor
{
    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public int InstructorRoleId { get; private set; }
    public InstructorRole Role { get; private set; }

    /// <summary>For deserialization only — do not call directly. Use <see cref="Create"/> or <see cref="Reconstitute"/>.</summary>
    [JsonConstructor]
    private Instructor(Guid id, string name, InstructorRole role)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", nameof(id));

        Id = id;
        SetValues(name, role);
    }

    public static Instructor Create(string name, InstructorRole role)
        => new(Guid.NewGuid(), name, role);

    public static Instructor Reconstitute(Guid id, string name, InstructorRole role)
        => new(id, name, role);

    public void Update(string name, InstructorRole role)
    {
        SetValues(name, role);
    }

    [MemberNotNull(nameof(Role))]
    private void SetValues(string name, InstructorRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

        ArgumentNullException.ThrowIfNull(role);

        if (role.Id <= 0)
            throw new ArgumentException("Instructor role ID must be greater than zero.", nameof(role));

        Name = name.Trim();
        Role = role;
        InstructorRoleId = role.Id;
    }
}
