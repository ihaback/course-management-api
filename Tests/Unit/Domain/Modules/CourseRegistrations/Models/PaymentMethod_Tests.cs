using Backend.Domain.Modules.PaymentMethods.Models;

namespace Backend.Tests.Unit.Domain.Modules.CourseRegistrations.Models;

public class PaymentMethod_Tests
{
    public static IEnumerable<object[]> ValidPaymentMethods()
    {
        yield return [PaymentMethod.Reconstitute(1, "Card")];
        yield return [PaymentMethod.Reconstitute(2, "Invoice")];
        yield return [PaymentMethod.Reconstitute(3, "Cash")];
        yield return [PaymentMethod.Reconstitute(0, "Unknown")];
    }

    [Theory]
    [MemberData(nameof(ValidPaymentMethods))]
    public void Constructor_Should_Create_PaymentMethod_When_Valid(PaymentMethod paymentMethod)
    {
        Assert.True(paymentMethod.Id >= 0);
        Assert.False(string.IsNullOrWhiteSpace(paymentMethod.Name));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Id_Is_Negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentMethod.Reconstitute(-1, "Invalid"));
    }
}
