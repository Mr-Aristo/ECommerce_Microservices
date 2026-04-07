namespace Order.Infrastructure.Data.Configurations;

/// <summary>
/// The ProductConfiguration class is responsible for configuring the Product entity in the database context.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(
            prodcutid => prodcutid.Value,
            dbId => ProductId.Of(dbId));

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
    }
}
