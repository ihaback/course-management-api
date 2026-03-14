using Backend.Domain.Common.ValueObjects;

namespace Backend.Tests.Unit.Domain.Common.ValueObjects;

public class Price_Tests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999.99)]
    [InlineData(99999.99)]
    public void Create_Should_Return_Price_When_Value_Is_Valid(decimal value)
    {
        var price = Price.Create(value);

        Assert.NotNull(price);
        Assert.Equal(value, price.Value);
    }

    [Fact]
    public void Create_Should_Accept_Zero()
    {
        var price = Price.Create(0m);

        Assert.Equal(0m, price.Value);
    }

    [Fact]
    public void Create_Should_Throw_ArgumentOutOfRangeException_When_Value_Is_Negative()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Price.Create(-0.01m));

        Assert.Contains("Price cannot be negative", ex.Message);
    }

    [Fact]
    public void Two_Prices_With_Same_Value_Should_Be_Equal()
    {
        var a = Price.Create(100m);
        var b = Price.Create(100m);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Two_Prices_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = Price.Create(100m);
        var b = Price.Create(200m);

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void ToString_Should_Return_Formatted_Value()
    {
        var price = Price.Create(100m);

        Assert.Equal("100", price.ToString());
    }

    [Fact]
    public void GetHashCode_Should_Be_Equal_For_Same_Value()
    {
        var a = Price.Create(100m);
        var b = Price.Create(100m);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
