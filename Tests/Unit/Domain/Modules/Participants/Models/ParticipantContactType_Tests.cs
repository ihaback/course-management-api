using Backend.Domain.Modules.ParticipantContactTypes.Models;

namespace Backend.Tests.Unit.Domain.Modules.Participants.Models;

public class ParticipantContactType_Tests
{
    public static IEnumerable<object[]> ValidContactTypes()
    {
        yield return [ParticipantContactType.Reconstitute(1, "Primary")];
        yield return [ParticipantContactType.Reconstitute(2, "Billing")];
        yield return [ParticipantContactType.Reconstitute(3, "Emergency")];
    }

    [Theory]
    [MemberData(nameof(ValidContactTypes))]
    public void Constructor_Should_Create_ContactType_When_Valid(ParticipantContactType contactType)
    {
        Assert.True(contactType.Id > 0);
        Assert.False(string.IsNullOrWhiteSpace(contactType.Name));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Id_Is_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ParticipantContactType.Reconstitute(0, "Invalid"));
    }
}
