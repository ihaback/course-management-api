using Backend.Domain.Common.ValueObjects;

namespace Backend.Tests.Unit.Domain.Common.ValueObjects;

public class Email_Tests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@example.co.uk")]
    [InlineData("alice.smith@subdomain.example.com")]
    public void Create_Should_Return_Email_When_Value_Is_Valid(string value)
    {
        var email = Email.Create(value);

        Assert.NotNull(email);
        Assert.Equal(value, email.Value);
    }

    [Fact]
    public void Create_Should_Trim_Whitespace()
    {
        var email = Email.Create("  user@example.com  ");

        Assert.Equal("user@example.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Throw_ArgumentException_When_Value_Is_Empty(string? value)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(value!));
    }

    [Theory]
    [InlineData("john.doe")]
    [InlineData("john.doe@")]
    [InlineData("@example.com")]
    [InlineData("john.doe@example")]
    [InlineData("john.doe@example.")]
    [InlineData("john doe@example.com")]
    public void Create_Should_Throw_ArgumentException_When_Format_Is_Invalid(string value)
    {
        var ex = Assert.Throws<ArgumentException>(() => Email.Create(value));

        Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Should_Preserve_Custom_ParamName_In_Exception()
    {
        var ex = Assert.Throws<ArgumentException>(() => Email.Create("", "email"));

        Assert.Equal("email", ex.ParamName);
    }

    [Fact]
    public void Two_Emails_With_Same_Value_Should_Be_Equal()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Two_Emails_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        var email = Email.Create("user@example.com");

        Assert.Equal("user@example.com", email.ToString());
    }

    [Fact]
    public void GetHashCode_Should_Be_Equal_For_Same_Value()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
