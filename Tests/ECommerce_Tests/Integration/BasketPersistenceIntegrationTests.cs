using BasketAPI.Data;
using BasketAPI.Models;
using Marten;
using Testcontainers.PostgreSql;

namespace ECommerce_Tests.Integration;

// Starts a throwaway PostgreSQL container for the test class. If Docker is unavailable the
// container fails to start and tests are skipped (so the suite stays green without Docker).
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string? ConnectionString { get; private set; }
    public bool DockerAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .Build();
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            DockerAvailable = true;
        }
        catch
        {
            DockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}

public class BasketPersistenceIntegrationTests(PostgresContainerFixture fixture)
    : IClassFixture<PostgresContainerFixture>
{
    [SkippableFact]
    public async Task StoreAndGetBasket_RoundTrips_AgainstRealPostgres()
    {
        Skip.IfNot(fixture.DockerAvailable, "Docker is not available; skipping Testcontainers integration test.");

        await using var store = DocumentStore.For(opts =>
        {
            opts.Connection(fixture.ConnectionString!);
            opts.Schema.For<ShoppingCard>().Identity(x => x.UserName);
        });
        await using var session = store.LightweightSession();
        var repository = new BasketRepository(session);

        var basket = new ShoppingCard
        {
            UserName = "integration-user",
            Items =
            [
                new ShoppingCardItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Integration-Product",
                    Quantity = 2,
                    Price = 50,
                    Color = "red"
                }
            ]
        };

        await repository.StoreBasket(basket);
        var loaded = await repository.GetBasket("integration-user");

        Assert.Equal("integration-user", loaded.UserName);
        Assert.Single(loaded.Items);
        Assert.Equal(100m, loaded.TotalPrice);
    }
}
