namespace Order.Domain.Models;

// A return request against a delivered order (persisted as a JSON column on Orders).
public class Return
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reason { get; set; } = default!;
    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
    public string? DecisionReason { get; set; }

    public Return() { }
    public Return(string reason) => Reason = reason;

    public void Approve() { Status = ReturnStatus.Approved; DecidedAt = DateTime.UtcNow; }
    public void Reject(string reason) { Status = ReturnStatus.Rejected; DecidedAt = DateTime.UtcNow; DecisionReason = reason; }
    public void MarkRefunded() => Status = ReturnStatus.Refunded;
}
