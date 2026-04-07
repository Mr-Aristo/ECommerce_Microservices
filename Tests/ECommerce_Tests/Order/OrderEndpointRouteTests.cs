using Order.API.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce_Tests.Order;

public class OrderEndpointRouteTests
{
    [Fact]
    public void CustomerAndNameEndpoints_ShouldHaveDistinctRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(Mock.Of<ISender>());

        var app = builder.Build();
        var byCustomer = new GetOrdersByCustomer();
        var byName = new GetOrdersByName();

        // Act
        byCustomer.AddRoutes(app);
        byName.AddRoutes(app);

        var routePatterns = app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText)
            .ToList();

        // Assert
        Assert.Contains("/orders/by-customer/{customerId:guid}", routePatterns);
        Assert.Contains("/orders/by-name/{orderName}", routePatterns);
        Assert.DoesNotContain("/orders/{customerId}", routePatterns);
        Assert.DoesNotContain("/orders/{orderName}", routePatterns);
    }
}
