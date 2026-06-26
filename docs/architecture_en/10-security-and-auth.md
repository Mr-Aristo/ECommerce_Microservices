# 10 — Security & Authentication / Authorization

This document consolidates the solution's security model in one place: how identity is proven
(authentication), what each principal may access (authorization), how abuse is throttled
(rate limiting), and how checkout/payment data is protected. It reflects the 2026-06-26
security audit outcomes (FIX-026..030).

> Generated from source; if it conflicts with the code, the code wins. Related task records:
> `docs/fixes/FIX-026..FIX-030` (VibeCode repo).

---

## 1. Authentication

- **Provider:** Keycloak (OIDC), realm `eshop`, public client `eshop-spa`.
  Exposed on `8088:8080` in compose; issuer host: `http://localhost:8088/realms/eshop`.
- **Token:** JWT Bearer access token. Every service and the gateway share the same validation:
  [`BuildingBlock/Auth/AuthenticationExtensions.cs`](../../Src/BuildingBlocks/BuildingBlock/Auth/AuthenticationExtensions.cs) →
  `AddStandardJwtAuth(configuration)`.
- **Validation parameters:**
  - `ValidateIssuer = true` (Authority = `Jwt:Authority`)
  - `ValidateLifetime = true`
  - `ValidateAudience` → on when `Jwt:Audience` is set. **FIX-029** sets `Jwt:Audience = eshop-api`
    on every service and adds an **audience mapper** (`oidc-audience-mapper`,
    `included.custom.audience = eshop-api`) to the Keycloak `eshop-spa` client, so the token's
    `aud` carries `eshop-api` and is validated. This stops tokens minted for a different client
    in the same realm from being accepted (audience confusion).
  - `RequireHttpsMetadata = false` — dev only (Keycloak over http in compose). TLS required in prod.
- **Role flow:** Keycloak roles arrive under `realm_access.roles`; the `MapRealmRoles` event
  flattens them into standard `ClaimTypes.Role` claims so `RequireRole(...)` works.
- **Identity propagation:** The user identity is **always** taken from the token (`sub`), never
  the client payload. Basket/Users/Order customer endpoints use `user.GetUserId()`; checkout
  sets `UserName`/`CustomerId` from the token `sub` (any client-supplied value is overwritten).

> **Where config lives:** `Jwt:Authority` + `Jwt:Audience` in `appsettings.json` for host-dev
> (Catalog/Users/Payment), and in `docker-compose.override.yml` env (`Jwt__Authority`,
> `Jwt__Audience`) for the dockerized run — Catalog, Basket, Order, Users, Payment and Gateway.

---

## 2. Roles

From the realm export (`keycloak/realms/eshop-realm.json`):

| Role | Meaning |
|---|---|
| `customer` | Registered shopper (own basket/orders/profile) |
| `support-agent` | Views/handles returns and customer orders |
| `fulfillment-manager` | Advances order status (process/ship/deliver) |
| `catalog-manager` | Manages products/coupons |
| `super-admin` | Full administration (incl. users/roles) |

Seed users: `customer1` (customer), `admin1` (all roles).

---

## 3. Authorization — per endpoint

Authorization is enforced **per service, per endpoint** (the gateway alone is not enough;
services are also reachable directly on their ports). The table reflects the state after FIX-026/027.

### Catalog (`:6000`)
| Endpoint | Policy |
|---|---|
| `GET /products*` (browse) | Anonymous (public read) |
| `POST /product-create`, `PUT /product-update`, `DELETE /product-delete/{id}` | `catalog-manager`, `super-admin` |

### Basket (`:6001`)
| Endpoint | Policy |
|---|---|
| `GET /basket`, `POST /basket-store`, `DELETE /basket/{}`, `POST /basket/checkout` | Authenticated (identity = token `sub`) |

### Users (`:6004`)
| Endpoint | Policy |
|---|---|
| `GET/PUT /users/me/profile`, addresses, favorites | Authenticated; own data only (`sub`) |

### Order (`:6003`) — **FIX-026**
| Endpoint | Policy |
|---|---|
| `GET /me/orders` | Authenticated (own orders only, `CustomerId == sub`) |
| `POST /me/orders/{id}/returns` | Authenticated (ownership checked in handler) |
| `GET /orders` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `GET /orders/by-customer/{customerId}` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `GET /orders/by-name/{orderName}` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `PUT /orders` | `fulfillment-manager`, `super-admin` |
| `DELETE /orders/{id}` | `super-admin` |
| `POST /orders/{id}/confirm\|process\|ship\|deliver\|cancel` | `fulfillment-manager`, `super-admin` |
| `POST /orders/{id}/returns/approve\|reject\|refund` | `support-agent`, `super-admin` |
| ~~`POST /orders`~~ | **Removed** — orders are created only by the checkout saga consumer (`BasketCheckoutEventHandler`) |

### Payment (`:6005`) — **FIX-027**
| Endpoint | Policy |
|---|---|
| `GET /payments/{orderId}` | `support-agent`, `super-admin` |

> **Before FIX-027** PaymentAPI had no auth wired at all; now `AddStandardJwtAuth` +
> `UseAuthentication/UseAuthorization` are in the pipeline and the endpoint is role-gated.

---

## 4. Gateway Edge Security

[`YarpApiGateway/Program.cs`](../../Src/ApiGateways/YarpApiGateway/Program.cs) + `appsettings.json` (Routes):

- **Edge authentication:** `basket`, `ordering`, `users` routes carry `AuthorizationPolicy: default`
  (authenticated user required). `catalog` is public (read).
- **Rate limiting (FIX-030):** Per-client **partitioned** limiter — key is the token `sub` when
  authenticated, else the client IP. A single abusive client can no longer drain one shared
  bucket and lock everyone out. On exceed: `429` + `Retry-After`.

  | Policy | Applied to route | Window / Limit |
  |---|---|---|
  | `fixed` (strict) | `ordering-route` | 10s / 5 |
  | `auth-sensitive` (moderate) | `basket-route`, `users-route` | 10s / 20 |
  | `catalog-loose` (loose) | `catalog-route` | 10s / 100 |

- **Real client IP:** `UseForwardedHeaders()` (X-Forwarded-For) resolves the real client behind a
  proxy into the partition. **Prod caveat:** restrict `KnownProxies/KnownNetworks` to the trusted
  load balancer range, otherwise X-Forwarded-For can be spoofed to bypass the partition.

> Services are also reachable directly on their ports (6000–6006), so authorization is enforced
> **inside each service** (defense against gateway bypass). In prod, do not expose service ports.

---

## 5. Checkout Idempotency (FIX-028)

`POST /basket/checkout` now accepts an optional **`Idempotency-Key`** HTTP header.

- Flow: [`CheckoutBasketEndpoints.cs`](../../Src/Services/Basket/BasketAPI/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs)
  reads the header → [`CheckoutBasketHandler`](../../Src/Services/Basket/BasketAPI/Basket/CheckoutBasket/CheckoutBasketHandler.cs)
  checks the key.
- If the key was already processed, it **replays** the first result (no second checkout/payment).
  Otherwise it starts the checkout and stores the key as an
  [`IdempotencyRecord`](../../Src/Services/Basket/BasketAPI/Models/IdempotencyRecord.cs)
  (Marten document, id = Idempotency-Key) **in the same transaction** as the basket + outbox.
- Without a key the legacy behavior is preserved (backward compatible). The existing
  `CheckoutPending` guard still blocks sequential repeats.
- **Message-level idempotency** also exists: `PaymentCaptureConsumer` and order creation are
  deterministic on `CheckoutId` → RabbitMQ redelivery does not produce duplicate payments/orders.
- **Deferred note:** Full Marten optimistic concurrency for the rare truly-simultaneous keyless
  checkout race is left to a follow-up, since it would change all `ShoppingCard` writes.

---

## 6. Payment Data Handling

- No real payment provider (PaymentAPI is mock). Full card number and CVV are **never stored**.
- The checkout DTO carries tokenized fields: `PaymentToken`, `PaymentReference`, `CardLast4`
  (last 4 only, enforced by validator regex), `CardBrand`. On the Order side `PaymentDataSanitizer`
  normalizes the token and redacts the CVV.
- `PaymentRecord` holds only `CustomerId`, `Amount`, `Status`; the endpoint is role-gated (Section 3).

---

## 7. Secrets Management

- DB/broker/Keycloak credentials come from `.env` (gitignored) + `docker-compose` env; the repo
  holds no real secrets (`.env.example` is sample/dev defaults only).
- Dev defaults (postgres/postgres, guest/guest, admin/admin) are **development only**; prod needs a
  secret store with rotation.

---

## 8. Open / Pre-Production Hardening

- HTTPS redirection / HSTS (FIX-014) — not yet; TLS termination at the gateway in prod.
- `AllowedHosts: *`, `RequireHttpsMetadata=false` — dev-acceptable, tighten for prod.
- Direct exposure of service ports — in prod only the gateway should be public.
- **FIX-029 runtime verification:** audience validation is fail-closed; with Keycloak running,
  obtain a token and confirm `aud=eshop-api` and that services accept it **before merging**.

Related: [08 — Gateway & Deployment](08-gateway-and-deployment.md) · [07 — Checkout Flow](07-checkout-flow.md)
