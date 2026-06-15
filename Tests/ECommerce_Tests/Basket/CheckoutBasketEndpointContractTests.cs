using BasketAPI.Basket.CheckoutBasket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce_Tests.Basket;

public class CheckoutBasketEndpointContractTests
{
    [Fact]
    public void CheckoutEndpoint_ShouldDeclare200Response()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(Mock.Of<ISender>());
        var app = builder.Build();

        var endpointModule = new CheckoutBasketEndpoints();
        endpointModule.AddRoutes(app);

        // Act
        // Read from the builder's own data sources; the DI-resolved EndpointDataSource is not
        // populated with minimal-API routes until the routing pipeline is built.
        var endpoint = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(e => e.RoutePattern.RawText == "/basket/checkout");

        var producesStatusCodes = endpoint.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .Select(m => m.StatusCode)
            .ToList();

        // Assert
        Assert.Contains(StatusCodes.Status200OK, producesStatusCodes);
        Assert.DoesNotContain(StatusCodes.Status201Created, producesStatusCodes);
    }
}
