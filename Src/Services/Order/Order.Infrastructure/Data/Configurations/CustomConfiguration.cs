
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Data.Configurations;

public class CustomConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(
            customerId => customerId.Value,
            dbId => CustomerId.Of(dbId));

        builder.Property(c=> c.Name).HasMaxLength(100).IsRequired();

        builder.Property(c => c.Email).HasMaxLength(250);

        builder.HasIndex(c => c.Email).IsUnique();
    }
}
/*
 HasMany/WithOne - one to many relation
 HasOna/WithMany - many to one relation

 */