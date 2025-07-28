namespace Order.Domain.Models;

//Order is rich-domain model


/// <summary>
/// Aggregate root representing an Order in the domain.
/// Encapsulates order details, related entities such as items and payment,
/// and enforces invariants through rich domain logic.
/// Follows the Domain-Driven Design (DDD) principles.
/// </summary>
public class Orders : Aggregate<OrderId>
{
    /// <summary>
    /// Internal list of items included in the order.
    /// </summary>
    private readonly List<OrderItem> _orderItems = new();

    /// <summary>
    /// Read-only collection of items in the order.
    /// </summary>
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    /// The ID of the customer who placed the order.
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>
    /// Descriptive name or reference for the order.
    /// </summary>
    public OrderName OrderName { get; private set; } = default!;

    /// <summary>
    /// Address where the order will be shipped.
    /// </summary>
    public Address ShippingAddress { get; private set; } = default!;

    /// <summary>
    /// Billing address associated with the order.
    /// </summary>
    public Address BillingAddress { get; private set; } = default!;

    /// <summary>
    /// Payment details used for the order.
    /// </summary>
    public Payment Payment { get; private set; } = default!;

    /// <summary>
    /// Current status of the order (e.g., Pending, Completed, Cancelled).
    /// </summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    /// <summary>
    /// Total calculated price of the order based on items and their quantities.
    /// </summary>
    public decimal TotalPrice
    {
        get => OrderItems.Sum(x => x.Price * x.Quantity);
        private set { }
    }

    /// <summary>
    /// Factory method to create a new order instance with required parameters.
    /// Raises a domain event (OrderCreatedEvent) upon creation.
    /// </summary>
    public static Orders Create(OrderId id, CustomerId customerId, OrderName orderName, Address shippingAddress, Address billingAddress, Payment payment)
    {
        var order = new Orders
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderCreatedEvent(order));

        return order;
    }

    /// <summary>
    /// Updates key properties of the order.
    /// Raises a domain event (OrderUpdatedEvent) to indicate state change.
    /// </summary>
    public void Update(OrderName orderName, Address shippingAddress, Address billingAddress, Payment payment, OrderStatus status)
    {
        OrderName = orderName;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Payment = payment;
        Status = status;

        AddDomainEvent(new OrderUpdatedEvent(this));
    }

    /// <summary>
    /// Adds a product to the order with specified quantity and price.
    /// Validates that quantity and price are positive values.
    /// </summary>
    public void Add(ProductId productId, int quantity, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var orderItem = new OrderItem(Id, productId, quantity, price);
        _orderItems.Add(orderItem);
    }

    /// <summary>
    /// Removes a product from the order if it exists in the item list.
    /// </summary>
    public void Remove(ProductId productId)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (orderItem is not null)
        {
            _orderItems.Remove(orderItem);
        }
    }
}
/*
  Private set amacin Order sinifinin ozelliklerinin Order sininfinin disinda
  bir degisiklige ugramaasinin onlemektir.   
*/