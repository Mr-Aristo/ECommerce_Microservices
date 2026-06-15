# 09 — Testing Strategy

Tests live under `Tests/ECommerce_Tests`, are written with **xUnit**, and run with `dotnet test`.

## Test Stack (ECommerce_Tests.csproj)

| Package | Purpose |
|---|---|
| `xunit 2.9.2` + `xunit.runner.visualstudio 2.8.2` | Test framework |
| `Microsoft.NET.Test.Sdk 17.12.0` | Test host |
| `Moq 4.20.72` | Mocking (e.g. `ISender`) |
| `Mapster 7.4.0` | When testing response mappings |
| `MediatR 12.5.0` | Handler/sender contracts |
| `Microsoft.EntityFrameworkCore.InMemory 9.0.2` | In-memory provider for EF Core |
| `coverlet.collector 6.0.2` | Code coverage |

The project references all services and building blocks, so handlers, validators, endpoint
logic, and consumers can be tested in isolation.

## Approach — Fast and Isolated from Infrastructure

Tests do not connect to real infrastructure (PostgreSQL, Redis, RabbitMQ, SQL Server);
dependencies are mocked or in-memory providers are used.

### Example: endpoint logic + Moq (`Basket/GetBasketEndpointTests.cs`)

```csharp
[Fact]
public async Task GetBasket_ReturnsOkResult_WithExpectedCart()
{
    var mockSender = new Mock<ISender>();
    var userName = "testUser";
    var expectedResult = new GetBasketResult(new ShoppingCard { UserName = userName, Items = [...] });

    mockSender.Setup(s => s.Send(It.Is<GetBasketQuery>(q => q.UserName == userName), default))
              .ReturnsAsync(expectedResult);

    var func = async (string u, ISender s) =>
        Results.Ok((await s.Send(new GetBasketQuery(u))).Adapt<GetBasketResponse>());

    var result = await func(userName, mockSender.Object);

    var ok = Assert.IsType<Ok<GetBasketResponse>>(result);
    Assert.Equal(userName, ok.Value.Cart.UserName);
    Assert.Equal(3, ok.Value.Cart.Items.Count);
}
```

### Example: validator test

```csharp
[Fact]
public void DeleteBasketCommandValidator_Should_HaveError_When_UserNameIsEmpty()
{
    var validator = new DeleteBasketCommandValidator();
    var result = validator.Validate(new DeleteBasketCommand(""));

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors,
        e => e.PropertyName == "UserName" && e.ErrorMessage == "UserName is required");
}
```

## What Is Tested at Each Layer?

| Layer | Test focus | Technique |
|---|---|---|
| Endpoint | The correct command/query reaches `ISender`, the correct `Results.Ok/Created` is returned | Moq `ISender` |
| Handler | Business logic, repository/session interaction | Mock repo / in-memory DbContext |
| Validator | FluentValidation rules (empty fields, bounds) | `validator.Validate(...)` |
| Consumer | Event → command/state transition (saga compensation) | Mock session/cache |
| Domain | Aggregate behavior, value object validation (`Of()`) | Pure unit test |

## Running

```bash
dotnet test                                   # all tests
dotnet test --collect:"XPlat Code Coverage"   # with coverage
```

> **Definition of done:** code compiles (`dotnet build`), tests pass (`dotnet test`),
> conventions are followed, and any new contract/route/migration is wired end-to-end.

Related: [02 — Building Blocks](02-building-blocks.md) · [04 — Basket](04-basket-service.md)
