# 05 — Discount Servisi (gRPC)

**Sorumluluk:** Kupon (indirim) yönetimi — contract-first, düşük gecikmeli senkron servis.
**Depolama:** SQLite (EF Core).
**Mimari stil:** gRPC servis (contract-first, `.proto`).
**Portlar:** Docker `6002` (HTTP) / `6062` (HTTPS), local `5002`.

---

## Klasör Yapısı

```text
DiscountGrpc/
├── Protos/discount.proto            (GrpcServices="Server")
├── Services/DiscountService.cs      (üretilen base class'ı implemente eder)
├── Models/Coupon.cs
├── Data/
│   ├── DiscountContext.cs           (EF Core DbContext + seed)
│   └── Extentions.cs                (UseMigration — başlangıçta migrate)
├── Migrations/
└── Program.cs
```

## Proto Sözleşmesi — `Protos/discount.proto`

```protobuf
syntax = "proto3";
option csharp_namespace = "DiscountGrpc.Protos";
package discount;
import "google/protobuf/empty.proto";

service DiscountProtoService {
    rpc GetDiscount     (GetDiscountRequest)    returns (CouponModel);
    rpc CreateDiscount  (CreateDiscountRequest) returns (CouponModel);
    rpc UpdateDiscount  (UpdateDiscountRequest) returns (CouponModel);
    rpc DeleteDiscount  (DeleteDiscountRequest) returns (DeleteDiscountResponse);
    rpc GetAllDiscounts (google.protobuf.Empty) returns (GetAllDiscountsResponse);
}

message CouponModel {
    int32 id = 1; string productName = 2; string description = 3; int32 amount = 4;
}
message GetDiscountRequest    { string productName = 1; }
message CreateDiscountRequest { CouponModel coupon = 1; }
message UpdateDiscountRequest { CouponModel coupon = 1; }
message DeleteDiscountRequest { string productName = 1; }
message DeleteDiscountResponse{ bool success = 1; }
message GetAllDiscountsResponse { repeated CouponModel coupons = 1; }
```

> `.csproj` içinde `<Protobuf Include="Protos\discount.proto" GrpcServices="Server" />` ile
> sunucu tarafı kod üretilir. Basket aynı `.proto`'yu `GrpcServices="Client"` ile kullanır.

## Servis Implementasyonu — `Services/DiscountService.cs`

Üretilen `DiscountProtoService.DiscountProtoServiceBase` sınıfını override eder; Mapster ile
`Coupon` ↔ `CouponModel` eşler.

| RPC | Davranış |
|---|---|
| `GetDiscount` | `ProductName` ile kupon arar; **yoksa `Amount=0` olan "No Discount" kuponu döner** (null değil) |
| `CreateDiscount` | Yeni kupon ekler; geçersizse `RpcException(InvalidArgument)` |
| `UpdateDiscount` | Kuponu günceller |
| `DeleteDiscount` | `ProductName` ile siler; yoksa `RpcException(InvalidArgument)` → `DeleteDiscountResponse{Success=true}` |
| `GetAllDiscounts` | Tüm kuponlar; `repeated` alan `RepeatedField<T>` olduğu için `AddRange` ile doldurulur |

```csharp
public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
{
    var coupon = await dbContext.Coupons
        .FirstOrDefaultAsync(x => x.ProductName == request.ProductName);

    coupon ??= new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };

    return coupon.Adapt<CouponModel>();
}
```

> **Önemli:** `GetDiscount` her zaman bir `CouponModel` döndürdüğü için, Basket tarafında
> kuponsuz ürünlerde `Amount = 0` düşülür — null kontrolü gerekmez.

## Veri Modeli — `Models/Coupon.cs`

```csharp
public class Coupon
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }
}
```

## EF Core / SQLite — `Data/DiscountContext.cs`

`DbSet<Coupon> Coupons`, `OnModelCreating` içinde `HasData(...)` ile 5 kupon seed'lenir
(IPhone X=150, Samsung 10=100, Samsung 12=200, Samsung 13=150, Samsung 14=100).

```csharp
// Program.cs
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));  // "Data Source=discountdb"

var app = builder.Build();
app.UseMigration();                       // başlangıçta Database.MigrateAsync()
app.MapGrpcService<DiscountService>();
app.Run();
```

`Extentions.UseMigration` uygulama açılışında bekleyen migration'ları uygular (SQLite dosyası
otomatik oluşur). Kestrel `Http1AndHttp2` ile yapılandırılır.

## Bağımlılıklar (DiscountGrpc.csproj)

`Grpc.AspNetCore 2.64.0`, `Mapster 7.4.0`, `Microsoft.EntityFrameworkCore.Sqlite 9.0.2`,
`Microsoft.EntityFrameworkCore.Tools 9.0.2`.

> Bu servis `BuildingBlock`'a bağımlı **değildir**; saf bir gRPC + EF Core servisidir.
> CQRS/MediatR kullanmaz.

Devamı: [06 — Order Servisi](06-order-service.md)
