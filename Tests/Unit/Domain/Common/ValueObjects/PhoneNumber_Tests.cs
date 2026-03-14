using Backend.Domain.Common.ValueObjects;

namespace Backend.Tests.Unit.Domain.Common.ValueObjects;

public class PhoneNumber_Tests
{
    [Theory]
    [InlineData("+46701234567")]
    [InlineData("0701234567")]
    [InlineData("+1-555-1234")]
    public void Create_Should_Return_PhoneNumber_When_Value_Is_Valid(string value)
    {
        var phone = PhoneNumber.Create(value);

        Assert.NotNull(phone);
        Assert.Equal(value, phone.Value);
    }

    [Fact]
    public void Create_Should_Trim_Whitespace()
    {
        var phone = PhoneNumber.Create("  +46701234567  ");

        Assert.Equal("+46701234567", phone.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_ArgumentException_When_Value_Is_Empty(string? value)
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Create(value!));
    }

    [Fact]
    public void Create_Should_Preserve_Custom_ParamName_In_Exception()
    {
        var ex = Assert.Throws<ArgumentException>(() => PhoneNumber.Create("", "phoneNumber"));

        Assert.Equal("phoneNumber", ex.ParamName);
    }

    [Fact]
    public void Two_PhoneNumbers_With_Same_Value_Should_Be_Equal()
    {
        var a = PhoneNumber.Create("+46701234567");
        var b = PhoneNumber.Create("+46701234567");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Two_PhoneNumbers_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = PhoneNumber.Create("+46701234567");
        var b = PhoneNumber.Create("+46709876543");

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        var phone = PhoneNumber.Create("+46701234567");

        Assert.Equal("+46701234567", phone.ToString());
    }

    [Fact]
    public void GetHashCode_Should_Be_Equal_For_Same_Value()
    {
        var a = PhoneNumber.Create("+46701234567");
        var b = PhoneNumber.Create("+46701234567");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
