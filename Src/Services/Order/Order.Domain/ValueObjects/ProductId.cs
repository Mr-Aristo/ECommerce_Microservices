
namespace Order.Domain.ValueObjects;
public record ProductId
{
    public Guid Value { get; }
    private ProductId(Guid value) => Value = value;
    public static ProductId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("ProductId cannot be empty.");
        }

        return new ProductId(value);
    }
}

/*
record: .NET’te immutable (değiştirilemez) nesneler tanımlamak için kullanılır. 
Value object'ler genellikle record olarak tanımlanır çünkü eşitlik kontrolü (equality) içerik bazlıdır (referans bazlı değil).

ProductId: Ürünün kimliğini temsil eder. Basitçe bir Guid değeri tutar ama doğrudan Guid yerine özel bir tip kullanılır.
 Böylece type-safety sağlanır (örneğin yanlışlıkla başka bir Guid buraya verilemez).
*/