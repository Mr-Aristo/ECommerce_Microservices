# 05 — Discount Service (gRPC)

**Responsibility:** Coupon (discount) management — a contract-first, low-latency synchronous service.
**Storage:** SQLite (EF Core).
**Architectural style:** gRPC service (contract-first, `.proto`).
**Ports:** Docker `6002` (HTTP) / `6062` (HTTPS), local `5002`.

---

## Folder Structure

```text
DiscountGrpc/
├── Protos/discount.proto            (GrpcServices="Server")
├── Services/DiscountService.cs      (implements the generated base class)
├── Models/Coupon.cs
├── Data/
│   ├── DiscountContext.cs           (EF Core DbContext + seed)
│   └── Extentions.cs                (UseMigration — migrate at startup)
├── Migrations/
└── Program.cs
```

## Proto Contract — `Protos/discount.proto`

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

> In the `.csproj`, `<Protobuf Include="Protos\discount.proto" GrpcServices="Server" />` generates
> the server-side code. Basket uses the same `.proto` with `GrpcServices="Client"`.

## Service Implementation — `Services/DiscountService.cs`

Overrides the generated `DiscountProtoService.DiscountProtoServiceBase`; maps `Coupon` ↔ `CouponModel` with Mapster.

| RPC | Behavior |
|---|---|
| `GetDiscount` | Looks up a coupon by `ProductName`; **if none, returns a "No Discount" coupon with `Amount=0`** (not null) |
| `CreateDiscount` | Adds a new coupon; if invalid, `RpcException(InvalidArgument)` |
| `UpdateDiscount` | Updates the coupon |
| `DeleteDiscount` | Deletes by `ProductName`; if missing, `RpcException(InvalidArgument)` → `DeleteDiscountResponse{Success=true}` |
| `GetAllDiscounts` | All coupons; since the `repeated` field is `RepeatedField<T>`, it is filled via `AddRange` |

```csharp
public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
{
    var coupon = await dbContext.Coupons
        .FirstOrDefaultAsync(x => x.ProductName == request.ProductName);

    coupon ??= new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };

    return coupon.Adapt<CouponModel>();
}
```

> **Important:** Because `GetDiscount` always returns a `CouponModel`, on the Basket side
> items without a coupon get `Amount = 0` deducted — no null check needed.

## Data Model — `Models/Coupon.cs`

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

`DbSet<Coupon> Coupons`; in `OnModelCreating`, 5 coupons are seeded via `HasData(...)`
(IPhone X=150, Samsung 10=100, Samsung 12=200, Samsung 13=150, Samsung 14=100).

```csharp
// Program.cs
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));  // "Data Source=discountdb"

var app = builder.Build();
app.UseMigration();                       // Database.MigrateAsync() at startup
app.MapGrpcService<DiscountService>();
app.Run();
```

`Extentions.UseMigration` applies pending migrations at startup (the SQLite file is created
automatically). Kestrel is configured with `Http1AndHttp2`.

## Dependencies (DiscountGrpc.csproj)

`Grpc.AspNetCore 2.64.0`, `Mapster 7.4.0`, `Microsoft.EntityFrameworkCore.Sqlite 9.0.2`,
`Microsoft.EntityFrameworkCore.Tools 9.0.2`.

> This service does **not** depend on `BuildingBlock`; it is a pure gRPC + EF Core service.
> It does not use CQRS/MediatR.

Next: [06 — Order Service](06-order-service.md)
