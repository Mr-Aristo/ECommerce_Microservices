using Order.Application.DTOs;
using Order.Application.OrdersCQRS.Commands.CreateOrder;
using Order.Domain.Enums;

namespace ECommerce_Tests.Order;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Validator_ShouldFail_WhenCustomerIdIsEmpty()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = CreateCommand(customerId: Guid.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CustomerId is required");
    }

    [Fact]
    public void Validator_ShouldFail_WhenAnyOrderItemHasInvalidQuantityOrPrice()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = CreateCommand(
            customerId: Guid.NewGuid(),
            items:
            [
                new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 0, 100),
                new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 1, 0)
            ]);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be greater than 0");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Price must be greater than 0");
    }

    [Fact]
    public void Validator_ShouldFail_WhenAddressOrPaymentFieldsAreMissing()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var order = new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderName: "ORD_1",
            ShippingAddress: new AddressDto("Jane", "Doe", "", "", "TR", "IST", "34000"),
            BillingAddress: new AddressDto("Jane", "Doe", "", "", "TR", "IST", "34000"),
            Payment: new PaymentDto("", "", "12/30", "", 1),
            Status: OrderStatus.Pending,
            OrderItems:
            [
                new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 1, 100)
            ]);
        var command = new CreateOrderCommand(order);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Shipping email is required");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Billing email is required");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardName is required");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CardNumber is required");
    }

    [Fact]
    public void Validator_ShouldFail_WhenCvvLengthGreaterThanThree()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var order = CreateCommand(Guid.NewGuid()).Order with
        {
            Payment = new PaymentDto("Jane Doe", "4111111111111111", "12/30", "1234", 1)
        };
        var command = new CreateOrderCommand(order);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CVV must be at most 3 characters");
    }

    private static CreateOrderCommand CreateCommand(Guid customerId, List<OrderItemDto>? items = null)
    {
        var orderItems = items ??
        [
            new OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), 1, 100)
        ];

        var order = new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: customerId,
            OrderName: "ORD_1",
            ShippingAddress: new AddressDto("Jane", "Doe", "jane@doe.com", "Main Street", "TR", "IST", "34000"),
            BillingAddress: new AddressDto("Jane", "Doe", "jane@doe.com", "Main Street", "TR", "IST", "34000"),
            Payment: new PaymentDto("Jane Doe", "4111111111111111", "12/30", "123", 1),
            Status: OrderStatus.Pending,
            OrderItems: orderItems);

        return new CreateOrderCommand(order);
    }
}
