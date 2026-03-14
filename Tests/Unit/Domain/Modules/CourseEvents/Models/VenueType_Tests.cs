using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseEvents.Models;

public class VenueType_Tests
{
    public static IEnumerable<object[]> ValidVenueTypes()
    {
        yield return [VenueType.Reconstitute(1, "InPerson")];
        yield return [VenueType.Reconstitute(2, "Online")];
        yield return [VenueType.Reconstitute(3, "Hybrid")];
    }

    [Theory]
    [MemberData(nameof(ValidVenueTypes))]
    public void Constructor_Should_Create_VenueType_When_Values_Are_Valid(VenueType venueType)
    {
        Assert.True(venueType.Id > 0);
        Assert.False(string.IsNullOrWhiteSpace(venueType.Name));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Id_Is_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VenueType.Reconstitute(0, "InPerson"));
    }
}
