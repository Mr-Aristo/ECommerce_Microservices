using Order.Application.OrdersCQRS.Commands.Returns;

namespace Order.API.Endpoints;

public record RequestReturnRequest(string Reason);
public record RejectReturnRequest(string Reason);

public class Returns : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Customer opens a return on their own delivered order (owner enforced in the handler).
        app.MapPost("/me/orders/{id:guid}/returns", async (Guid id, RequestReturnRequest body, ClaimsPrincipal user, ISender sender) =>
        {
            var customerId = Guid.TryParse(user.GetUserId(), out var cid) ? cid : Guid.Empty;
            var result = await sender.Send(new RequestReturnCommand(id, customerId, body.Reason));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("RequestReturn")
        .WithSummary("Open a return on my order");

        // Support/admin: decide on the return and complete the refund.
        var admin = app.MapGroup("/orders/{id:guid}/returns")
            .RequireAuthorization(policy => policy.RequireRole("support-agent", "super-admin"))
            .WithTags("Returns (admin)");

        admin.MapPost("/approve", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new ApproveReturnCommand(id))));

        admin.MapPost("/reject", async (Guid id, RejectReturnRequest body, ISender sender) =>
            Results.Ok(await sender.Send(new RejectReturnCommand(id, body.Reason))));

        admin.MapPost("/refund", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new CompleteRefundCommand(id))));
    }
}
