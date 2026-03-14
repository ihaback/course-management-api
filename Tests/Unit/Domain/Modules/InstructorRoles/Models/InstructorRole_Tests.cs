using Backend.Domain.Modules.InstructorRoles.Models;

namespace Backend.Tests.Unit.Domain.Modules.InstructorRoles.Models;

public class InstructorRole_Tests
{
    [Fact]
    public void Constructor_Should_Create_Role_When_Valid()
    {
        var role = InstructorRole.Reconstitute(1, "Lead");

        Assert.Equal(1, role.Id);
        Assert.Equal("Lead", role.Name);
    }

    [Fact]
    public void Constructor_Should_Trim_Name()
    {
        var role = InstructorRole.Reconstitute(2, "  Assistant  ");

        Assert.Equal("Assistant", role.Name);
    }

    [Fact]
    public void Constructor_Should_Accept_Id_Zero_For_New_Role()
    {
        var role = InstructorRole.Reconstitute(0, "Lead");

        Assert.Equal(0, role.Id);
        Assert.Equal("Lead", role.Name);
    }

    [Theory]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_Id_Invalid(int id)
    {
        var ex = Assert.Throws<ArgumentException>(() => InstructorRole.Reconstitute(id, "Lead"));
        Assert.Equal("id", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_Throw_When_Name_Invalid(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => InstructorRole.Reconstitute(1, name!));
        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Update_Should_Change_RoleName_When_Input_Is_Valid()
    {
        var role = InstructorRole.Reconstitute(1, "Lead");

        role.Update("Assistant");

        Assert.Equal(1, role.Id);
        Assert.Equal("Assistant", role.Name);
    }

    [Fact]
    public void Update_Should_Throw_When_Name_Is_Whitespace()
    {
        var role = InstructorRole.Reconstitute(1, "Lead");

        var ex = Assert.Throws<ArgumentException>(() => role.Update("   "));
        Assert.Equal("name", ex.ParamName);
    }
}
