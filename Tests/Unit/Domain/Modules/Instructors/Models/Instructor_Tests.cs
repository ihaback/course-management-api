using Backend.Domain.Modules.InstructorRoles.Models;
using Backend.Domain.Modules.Instructors.Models;

namespace Backend.Tests.Unit.Domain.Modules.Instructors.Models;

public class Instructor_Tests
{
    [Fact]
    public void Constructor_Should_Create_Instructor_When_Parameters_Are_Valid()
    {
        var id = Guid.NewGuid();
        var name = "John Doe";
        var role = InstructorRole.Reconstitute(1, "Lead");

        var instructor = Instructor.Reconstitute(id, name, role);

        Assert.Equal(id, instructor.Id);
        Assert.Equal(name, instructor.Name);
        Assert.Equal(role.Id, instructor.InstructorRoleId);
        Assert.Equal(role, instructor.Role);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Id_Is_Empty()
    {
        var ex = Assert.Throws<ArgumentException>(() => Instructor.Reconstitute(Guid.Empty, "Jane", InstructorRole.Reconstitute(1, "Lead")));
        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_Throw_When_Name_Is_Invalid(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Instructor.Reconstitute(Guid.NewGuid(), name!, InstructorRole.Reconstitute(1, "Lead")));
        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Role_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => Instructor.Reconstitute(Guid.NewGuid(), "Jane", null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Role_Id_Is_Zero()
    {
        var role = InstructorRole.Reconstitute(0, "Lead");

        var ex = Assert.Throws<ArgumentException>(() => Instructor.Reconstitute(Guid.NewGuid(), "Jane", role));
        Assert.Equal("role", ex.ParamName);
    }

    [Fact]
    public void Constructor_Should_Trim_Name()
    {
        var id = Guid.NewGuid();
        var role = InstructorRole.Reconstitute(1, "Lead");

        var instructor = Instructor.Reconstitute(id, "  Jane  ", role);

        Assert.Equal("Jane", instructor.Name);
    }

    [Fact]
    public void Update_Should_Change_Name_And_Role_When_Input_Is_Valid()
    {
        var instructor = Instructor.Reconstitute(Guid.NewGuid(), "Jane", InstructorRole.Reconstitute(1, "Lead"));
        var newRole = InstructorRole.Reconstitute(2, "Assistant");

        instructor.Update("John", newRole);

        Assert.Equal("John", instructor.Name);
        Assert.Equal(2, instructor.InstructorRoleId);
        Assert.Equal(newRole, instructor.Role);
    }

    [Fact]
    public void Update_Should_Throw_When_Role_Id_Is_Zero()
    {
        var instructor = Instructor.Reconstitute(Guid.NewGuid(), "Jane", InstructorRole.Reconstitute(1, "Lead"));
        var invalidRole = InstructorRole.Reconstitute(0, "Assistant");

        var ex = Assert.Throws<ArgumentException>(() => instructor.Update("John", invalidRole));
        Assert.Equal("role", ex.ParamName);
    }
}
