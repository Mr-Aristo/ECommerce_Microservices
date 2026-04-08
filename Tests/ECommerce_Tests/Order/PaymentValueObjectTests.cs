using Order.Domain.ValueObjects;

namespace ECommerce_Tests.Order;

public class PaymentValueObjectTests
{
    [Fact]
    public void Of_ShouldPersistProvidedPaymentToken_AndRedactedCvv()
    {
        // Act
        var payment = Payment.Of("Jane Doe", "tok_abc_123", "pi_123", "123", 1);

        // Assert
        Assert.Equal("tok_abc_123", payment.CardNumber);
        Assert.Equal("***", payment.CVV);
    }
}
