using BuildingBlock.Exceptions;
using BuildingBlockMessaging.Events;
using Microsoft.Extensions.Caching.Distributed;
using BasketAPI.Exceptions;

namespace BasketAPI.Basket.CheckoutBasket;

// Note: DB write (basket + outbox + idempotency) is atomic via a single SaveChangesAsync.
// The Redis cache write after it is best-effort (cache is a read-through copy, rebuilt on miss).
// Idempotency-Key replays a prior checkout; full optimistic-concurrency hardening for the rare
// truly-simultaneous keyless checkout is tracked as a follow-up (would change all ShoppingCard writes).

// Command & Handler Records
// IdempotencyKey is the client-supplied "Idempotency-Key" header (optional, null when absent).
public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto, string? IdempotencyKey = null) : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);
    
//FluentValidation
public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand> //FluentValidation
{
	public CheckoutBasketCommandValidator()
	{
        //x."variableName" must be the same as AbstractValidator<"Type's parameters name which is CheckoutBasketCommand">
        RuleFor(x => x.BasketCheckoutDto).NotNull().WithMessage("BasketCheckoutDto cannot be null.");
        RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required.");
        RuleFor(x => x.BasketCheckoutDto.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.BasketCheckoutDto.EmailAddress).NotEmpty().WithMessage("EmailAddress is required.");
        RuleFor(x => x.BasketCheckoutDto.AddressLine).NotEmpty().WithMessage("AddressLine is required.");
        RuleFor(x => x.BasketCheckoutDto.CardName).NotEmpty().WithMessage("CardName is required.");
        RuleFor(x => x.BasketCheckoutDto.PaymentToken).NotEmpty().WithMessage("PaymentToken is required.");
        RuleFor(x => x.BasketCheckoutDto.PaymentReference).NotEmpty().WithMessage("PaymentReference is required.");
        RuleFor(x => x.BasketCheckoutDto.CardLast4)
            .NotEmpty().WithMessage("CardLast4 is required.")
            .Length(4).WithMessage("CardLast4 must be exactly 4 characters.")
            .Matches(@"^\d{4}$").WithMessage("CardLast4 must contain only digits.");
        RuleFor(x => x.BasketCheckoutDto.CardBrand).NotEmpty().WithMessage("CardBrand is required.");
	}
}

public class CheckoutBasketHandler(IDocumentSession session, IDistributedCache cache) : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        // IDEMPOTENCY: a retried checkout with the same key replays the original result
        // instead of starting a second checkout (and a second payment downstream).
        var idempotencyKey = command.IdempotencyKey;
        // Scope the key to the authenticated user (UserName = token sub) so two users sending the
        // same Idempotency-Key value can't collide — otherwise user B replaying user A's key would
        // get a false success and B's own basket would never be checked out.
        var scopedKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : $"{command.BasketCheckoutDto.UserName}:{idempotencyKey}";
        if (scopedKey is not null)
        {
            var priorCheckout = await session.LoadAsync<IdempotencyRecord>(scopedKey, cancellationToken);
            if (priorCheckout is not null)
            {
                return new CheckoutBasketResult(priorCheckout.IsSuccess);
            }
        }

        var basket = await session.LoadAsync<ShoppingCard>(command.BasketCheckoutDto.UserName, cancellationToken)
            ?? throw new BasketNotFoundException(command.BasketCheckoutDto.UserName);

        if (!basket.Items.Any())
        {
            throw new BadRequestException("Checkout cannot proceed with an empty basket.");
        }

        if (basket.Status == BasketStatus.CheckoutPending)
        {
            throw new BadRequestException("Checkout is already pending for this basket.");
        }

        var checkoutId = Guid.NewGuid();
        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.CheckoutId = checkoutId;
        eventMessage.TotalPrice = basket.TotalPrice;
        eventMessage.Items = basket.Items
            .Select(item => new BasketCheckoutItemEvent
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price
            })
            .ToList();

        var outboxMessage = new BasketCheckoutOutboxMessage
        {
            CheckoutId = checkoutId,
            UserName = basket.UserName,
            Payload = eventMessage
        };

        // OUTBOX/SAGA: mark basket as pending before event publish; final state comes from success/fail events.
        basket.Status = BasketStatus.CheckoutPending;
        basket.PendingCheckoutId = checkoutId;

        // OUTBOX/SAGA: persist integration event to local outbox in same DB transaction as basket state change.
        session.Store(basket);
        session.Store(outboxMessage);

        // IDEMPOTENCY: record the key in the SAME transaction so a retry replays this checkout.
        if (scopedKey is not null)
        {
            session.Store(new IdempotencyRecord
            {
                Key = scopedKey,
                CheckoutId = checkoutId,
                UserName = basket.UserName,
                IsSuccess = true
            });
        }

        await session.SaveChangesAsync(cancellationToken);

        await cache.SetStringAsync(
            basket.UserName,
            JsonSerializer.Serialize(basket),
            cancellationToken);

        return new CheckoutBasketResult(true);
    }
}
