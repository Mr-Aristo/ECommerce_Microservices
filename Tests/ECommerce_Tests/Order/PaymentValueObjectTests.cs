using Order.Domain.ValueObjects;

namespace ECommerce_Tests.Order;

public class PaymentValueObjectTests
{
    [Fact]
    public void Of_ShouldPersistMaskedCardNumber_AndRedactedCvv()
    {
        // Act
        var payment = Payment.Of("Jane Doe", "4111111111111111", "12/30", "123", 1);

        // Assert
        Assert.Equal("**** **** **** 1111", payment.CardNumber);
        Assert.Equal("***", payment.CVV);
    }
}
