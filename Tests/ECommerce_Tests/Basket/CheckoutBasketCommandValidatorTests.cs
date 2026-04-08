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
            CardNumber = "",
            CVV = ""
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
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardNumber is required.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenCvvLengthGreaterThanThree()
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
            CardNumber = "4111111111111111",
            CVV = "1234"
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CVV must be at most 3 characters.");
    }
}
