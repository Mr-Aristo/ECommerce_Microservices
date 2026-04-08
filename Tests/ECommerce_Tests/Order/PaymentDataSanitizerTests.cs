using Order.Application.Security;

namespace ECommerce_Tests.Order;

public class PaymentDataSanitizerTests
{
    [Theory]
    [InlineData("4111111111111111", "**** **** **** 1111")]
    [InlineData("**** **** **** 4242", "**** **** **** 4242")]
    [InlineData("123", "****")]
    [InlineData("tok_abc_123", "tok_abc_123")]
    public void NormalizePaymentToken_ShouldReturnExpectedValue(string input, string expected)
    {
        // Act
        var result = PaymentDataSanitizer.NormalizePaymentToken(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MaskCardNumber_ShouldUseNormalizePaymentTokenForBackwardCompatibility()
    {
        // Act
        var result = PaymentDataSanitizer.MaskCardNumber("tok_legacy_123");

        // Assert
        Assert.Equal("tok_legacy_123", result);
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
