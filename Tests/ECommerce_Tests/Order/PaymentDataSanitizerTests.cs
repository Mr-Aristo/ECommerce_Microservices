using Order.Application.Security;

namespace ECommerce_Tests.Order;

public class PaymentDataSanitizerTests
{
    [Theory]
    [InlineData("4111111111111111", "**** **** **** 1111")]
    [InlineData("**** **** **** 4242", "**** **** **** 4242")]
    [InlineData("123", "****")]
    [InlineData("abcd", "****")]
    public void MaskCardNumber_ShouldReturnExpectedMask(string input, string expected)
    {
        // Act
        var result = PaymentDataSanitizer.MaskCardNumber(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RedactCvv_ShouldAlwaysReturnRedactedValue()
    {
        // Act
        var result = PaymentDataSanitizer.RedactCvv();

        // Assert
        Assert.Equal("***", result);
    }
}
