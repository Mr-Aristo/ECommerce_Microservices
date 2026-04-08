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

        var routeEndpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        // Assert
        var byCustomerEndpoint = routeEndpoints.SingleOrDefault(e =>
            string.Equals(
                e.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName,
                "GetOrdersByCustomer",
                StringComparison.Ordinal));

        var byNameEndpoint = routeEndpoints.SingleOrDefault(e =>
            string.Equals(
                e.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName,
                "GetOrdersByName",
                StringComparison.Ordinal));

        Assert.NotNull(byCustomerEndpoint);
        Assert.NotNull(byNameEndpoint);

        var byCustomerTemplate = (byCustomerEndpoint!.RoutePattern.RawText ?? byCustomerEndpoint.RoutePattern.ToString()).TrimStart('/');
        var byNameTemplate = (byNameEndpoint!.RoutePattern.RawText ?? byNameEndpoint.RoutePattern.ToString()).TrimStart('/');

        Assert.StartsWith("orders/by-customer/", byCustomerTemplate, StringComparison.Ordinal);
        Assert.Contains("{customerId", byCustomerTemplate, StringComparison.Ordinal);
        Assert.StartsWith("orders/by-name/", byNameTemplate, StringComparison.Ordinal);
        Assert.Contains("{orderName", byNameTemplate, StringComparison.Ordinal);
        Assert.NotEqual(byCustomerTemplate, byNameTemplate);

        var customerIdParameter = byCustomerEndpoint.RoutePattern.Parameters.Single(p => p.Name == "customerId");
        Assert.Contains(customerIdParameter.ParameterPolicies, p => p.Content == "guid");
    }
}
