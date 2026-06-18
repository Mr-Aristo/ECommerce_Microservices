namespace Order.Domain.Events;

/// <summary>Raised when a customer opens a return request on a delivered order.</summary>
public record ReturnRequestedEvent(Orders order) : IDomainEvent;

/// <summary>Raised when support approves a return; the Payment service refunds against this.</summary>
public record ReturnApprovedEvent(Orders order) : IDomainEvent;

/// <summary>Raised when the refund completes and the order is marked Returned.</summary>
public record OrderReturnedEvent(Orders order) : IDomainEvent;
