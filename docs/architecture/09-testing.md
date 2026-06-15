# 09 — Test Stratejisi

Testler `Tests/ECommerce_Tests` altında, **xUnit** ile yazılır ve `dotnet test` ile çalışır.

## Test Yığını (ECommerce_Tests.csproj)

| Paket | Amaç |
|---|---|
| `xunit 2.9.2` + `xunit.runner.visualstudio 2.8.2` | Test framework |
| `Microsoft.NET.Test.Sdk 17.12.0` | Test host |
| `Moq 4.20.72` | Mock'lama (örn. `ISender`) |
| `Mapster 7.4.0` | Response eşlemelerini test ederken |
| `MediatR 12.5.0` | Handler/sender sözleşmeleri |
| `Microsoft.EntityFrameworkCore.InMemory 9.0.2` | EF Core için in-memory sağlayıcı |
| `coverlet.collector 6.0.2` | Kod kapsamı |

Proje, tüm servislere ve building block'lara referans verir; böylece handler, validator,
endpoint mantığı ve consumer'lar izole test edilebilir.

## Yaklaşım — Hızlı ve Altyapıdan İzole

Testler gerçek altyapıya (PostgreSQL, Redis, RabbitMQ, SQL Server) bağlanmaz; bağımlılıklar
mock'lanır veya in-memory sağlayıcılar kullanılır.

### Örnek: Endpoint mantığı + Moq (`Basket/GetBasketEndpointTests.cs`)

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

### Örnek: Validator testi

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

## Katman Bazlı Ne Test Edilir?

| Katman | Test odağı | Teknik |
|---|---|---|
| Endpoint | `ISender`'a doğru command/query gider, doğru `Results.Ok/Created` döner | Moq `ISender` |
| Handler | İş mantığı, repository/session etkileşimi | Mock repo / in-memory DbContext |
| Validator | FluentValidation kuralları (boş alan, sınırlar) | `validator.Validate(...)` |
| Consumer | Event → komut/durum geçişi (saga telafisi) | Mock session/cache |
| Domain | Aggregate davranışı, value object doğrulaması (`Of()`) | Saf birim test |

## Çalıştırma

```bash
dotnet test                                   # tüm testler
dotnet test --collect:"XPlat Code Coverage"   # kapsam ile
```

> **Definition of done:** kod derlenir (`dotnet build`), testler geçer (`dotnet test`),
> konvansiyonlar izlenir ve yeni her contract/route/migration uçtan uca bağlanır.

İlgili: [02 — Building Blocks](02-building-blocks.md) · [04 — Basket](04-basket-service.md)
