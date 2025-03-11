namespace Order.Application.OrdersCQRS.Commands.DeleteOrder;

public class DeleteOrderHandler(IApplicationDbContext dbcontext) : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);
        var order =  await dbcontext.Orders
            .FindAsync([orderId], cancellationToken);
        if (order == null)
            throw new OrderNotFoundException(request.OrderId);

        dbcontext.Orders.Remove(order);
        await dbcontext.SaveChangesAsync(cancellationToken);

        return new DeleteOrderResult(true);
    }
}
/*
Neden Köşeli Parantez([]) Kullanılıyor?
FindAsync metodu, birincil anahtar(primary key) kullanarak veritabanında bir nesne aramak için kullanılır.Ancak, birincil anahtar birden fazla kolon içerebilir(Composite Key).
Eğer birincil anahtar tek bir sütundan oluşuyorsa, tek başına bir değer geçilebilir.
Eğer birden fazla sütundan(Composite Key) oluşuyorsa, bir dizi veya koleksiyon içinde verilmesi gerekir.
FindAsync metodu, parametre olarak object?[] keyValues(nullable object array) beklediği için tek bir anahtar bile olsa dizi olarak verilmesi mümkündür.
*/