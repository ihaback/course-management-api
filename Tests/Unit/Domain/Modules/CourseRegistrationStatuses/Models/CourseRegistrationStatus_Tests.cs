using Backend.Domain.Modules.CourseRegistrationStatuses.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseRegistrationStatuses.Models;

public class CourseRegistrationStatus_Tests
{
    [Fact]
    public void Constructor_Should_Create_Status_When_Valid()
    {
        var status = CourseRegistrationStatus.Reconstitute(1, "Paid");

        Assert.Equal(1, status.Id);
        Assert.Equal("Paid", status.Name);
    }

    [Fact]
    public void Update_Should_Change_Name_When_Input_Is_Valid()
    {
        var status = CourseRegistrationStatus.Reconstitute(1, "Paid");

        status.Update("Refunded");

        Assert.Equal(1, status.Id);
        Assert.Equal("Refunded", status.Name);
    }

    [Fact]
    public void Update_Should_Throw_ArgumentException_When_Name_Is_Whitespace()
    {
        var status = CourseRegistrationStatus.Reconstitute(1, "Paid");

        var ex = Assert.Throws<ArgumentException>(() => status.Update("   "));
        Assert.Equal("name", ex.ParamName);
    }
}
