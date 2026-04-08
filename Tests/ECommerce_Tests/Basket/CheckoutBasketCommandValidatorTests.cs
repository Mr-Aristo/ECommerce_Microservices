using BasketAPI.Basket.CheckoutBasket;
using BasketAPI.DTOs;

namespace ECommerce_Tests.Basket;

public class CheckoutBasketCommandValidatorTests
{
    [Fact]
    public void Validator_ShouldFail_WhenRequiredFieldsMissing()
    {
        // Arrange
        var validator = new CheckoutBasketCommandValidator();
        var command = new CheckoutBasketCommand(new BasketCheckoutDto
        {
            UserName = "",
            CustomerId = Guid.Empty,
            EmailAddress = "",
            AddressLine = "",
            CardName = "",
            PaymentToken = "",
            PaymentReference = "",
            CardLast4 = "",
            CardBrand = ""
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "UserName is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CustomerId is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "EmailAddress is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "AddressLine is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardName is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "PaymentToken is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "PaymentReference is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardLast4 is required.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardBrand is required.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenCardLast4LengthIsNotFour()
    {
        // Arrange
        var validator = new CheckoutBasketCommandValidator();
        var command = new CheckoutBasketCommand(new BasketCheckoutDto
        {
            UserName = "user-1",
            CustomerId = Guid.NewGuid(),
            EmailAddress = "user@test.com",
            AddressLine = "Main Street",
            CardName = "User Test",
            PaymentToken = "tok_123456",
            PaymentReference = "pi_123",
            CardLast4 = "123",
            CardBrand = "VISA"
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardLast4 must be exactly 4 characters.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenCardLast4HasNonDigitCharacters()
    {
        // Arrange
        var validator = new CheckoutBasketCommandValidator();
        var command = new CheckoutBasketCommand(new BasketCheckoutDto
        {
            UserName = "user-1",
            CustomerId = Guid.NewGuid(),
            EmailAddress = "user@test.com",
            AddressLine = "Main Street",
            CardName = "User Test",
            PaymentToken = "tok_123456",
            PaymentReference = "pi_123",
            CardLast4 = "12A4",
            CardBrand = "VISA"
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardLast4 must contain only digits.");
    }
}
