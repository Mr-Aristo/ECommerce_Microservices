using Microsoft.EntityFrameworkCore;
using Order.Domain.Abstractions;
using Order.Infrastructure.Data.Interceptors;
using System.Reflection;

namespace ECommerce_Tests.Order;

public class AuditableEntityInterceptorTests
{
    [Fact]
    public void UpdateEntities_ShouldPopulateAuditFields_ForAddedEntities()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        // Use reflection to access the private UpdateEntities method
        var method = typeof(AuditableEntityInterceptor).GetMethod("UpdateEntities", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var options = new DbContextOptionsBuilder<FakeAuditableDbContext>().Options;
        using var context = new FakeAuditableDbContext(options);

        var entity = new FakeAuditableEntity { Id = Guid.NewGuid() };
        context.Entities.Add(entity);

        // Act
        method!.Invoke(interceptor, [context]);

        // Assert
        Assert.Equal("Emre", entity.CreatedBy);
        Assert.NotNull(entity.CreatedAt);
        Assert.Equal("Emre", entity.LastModifiedBy);
        Assert.NotNull(entity.LastModified);
    }

    [Fact]
    public void UpdateEntities_ShouldNotThrow_WhenContextIsNull()
    {
        // Arrange
        var interceptor = new AuditableEntityInterceptor();
        var method = typeof(AuditableEntityInterceptor).GetMethod("UpdateEntities", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        // Act + Assert
        var ex = Record.Exception(() => method!.Invoke(interceptor, [null]));
        Assert.Null(ex);
    }

    private sealed class FakeAuditableDbContext(DbContextOptions<FakeAuditableDbContext> options) : DbContext(options)
    {
        public DbSet<FakeAuditableEntity> Entities => Set<FakeAuditableEntity>();
    }

    private sealed class FakeAuditableEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
