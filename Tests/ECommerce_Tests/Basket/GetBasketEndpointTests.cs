using BasketAPI.Basket.DeleteBasket;

namespace ECommerce_Tests.Basket;

public class GetBasketEndpointTests
{
    [Fact]
    public async Task GetBasket_ReturnsOkResult_WithExpectedCart()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var userName = "testUser";

        var expectedShoppingCart = new ShoppingCard
        {
            UserName = userName,
            Items = new List<ShoppingCardItem> {
                new ShoppingCardItem
                {
                    Quantity = 1,
                    Color = "red",
                    Price = 533,
                    ProductId = new Guid("5940f0aa-4274-4b6a-9876-d1544148213c"),
                    ProductName = "Samsung3"
                },
                new ShoppingCardItem
                {
                    Quantity = 1,
                    Color = "red",
                    Price = 600,
                    ProductId = new Guid("6140f0aa-4274-4b6a-9876-d1544148213c"),
                    ProductName = "Samsung44"
                },
                new ShoppingCardItem
                {
                    Quantity = 1,
                    Color = "red",
                    Price = 7790,
                    ProductId = new Guid("6040f0aa-4274-4b6a-9876-d1544148213c"),
                    ProductName = "Samsung45"
                }
            }
        };

        var expectedResult = new GetBasketResult(expectedShoppingCart);

        mockSender.Setup(s => s.Send(It.Is<GetBasketQuery>(q => q.UserName == userName), default))
         .ReturnsAsync(expectedResult);


        var fucn = async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new GetBasketQuery(userName));

            var response = result.Adapt<GetBasketResponse>();

            return Results.Ok(response);
        };


        // Act

        var result = await fucn(userName, mockSender.Object);


        // Assert 

        var okResult = Assert.IsType<Ok<GetBasketResponse>>(result);
        Assert.Equal(userName, okResult.Value.Cart.UserName);
        Assert.Equal(3, okResult.Value.Cart.Items.Count);

    }

    [Fact]
    public async Task BasketDelete_ReturnsOk()
    {
        // Arrange 
        var mockSender = new Mock<ISender>();
        var userName = "testUser";

        var expectedResult = new DeleteBasketResult(true);

        var func = async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new DeleteBasketCommand(userName));

            var response = result.Adapt<DeleteBasketResponse>();

            return Results.Ok(response);
        };

        mockSender.Setup(s => s.Send(It.Is<DeleteBasketCommand>(q => q.UserName == userName), default)).ReturnsAsync(expectedResult);

        // Act

        var result = await func(userName, mockSender.Object);

        // Assert 
        var okResult = Assert.IsType<Ok<DeleteBasketResponse>>(result);

    }

    [Fact]
    public void DeleteBasketCommandValidator_Should_HaveError_When_UserNameIsEmpty()
    {
        string userName = "";
        // Arrange
        var validator = new DeleteBasketCommandValidator();
        var command = new DeleteBasketCommand(userName);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName" && e.ErrorMessage == "UserName is required");
    }

}
