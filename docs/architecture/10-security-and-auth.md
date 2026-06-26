# 10 — Güvenlik & Kimlik Doğrulama / Yetkilendirme

Bu belge, çözümün güvenlik modelini tek yerde toplar: kim kimliğini nasıl kanıtlar
(authentication), neye erişebilir (authorization), kötüye kullanım nasıl sınırlanır
(rate limiting), ve checkout/ödeme verisi nasıl korunur. 2026-06-26 güvenlik denetiminin
(FIX-026..030) sonuçları buraya işlenmiştir.

> Kaynaktan üretilmiştir; kod ile çelişki olursa kod esastır. İlgili görev kayıtları:
> `docs/fixes/FIX-026..FIX-030` (VibeCode repo).

---

## 1. Kimlik Doğrulama (Authentication)

- **Sağlayıcı:** Keycloak (OIDC), realm `eshop`, public client `eshop-spa`.
  Compose'da `8088:8080` portunda; issuer host: `http://localhost:8088/realms/eshop`.
- **Token tipi:** JWT Bearer access token. Her servis ve gateway aynı paylaşılan doğrulamayı
  kullanır: [`BuildingBlock/Auth/AuthenticationExtensions.cs`](../../Src/BuildingBlocks/BuildingBlock/Auth/AuthenticationExtensions.cs) →
  `AddStandardJwtAuth(configuration)`.
- **Doğrulama parametreleri:**
  - `ValidateIssuer = true` (Authority = `Jwt:Authority`)
  - `ValidateLifetime = true`
  - `ValidateAudience` → `Jwt:Audience` doluysa açıktır. **FIX-029** ile her servise
    `Jwt:Audience = eshop-api` verildi ve Keycloak `eshop-spa` client'ına bir
    **audience mapper** (`oidc-audience-mapper`, `included.custom.audience = eshop-api`)
    eklendi; böylece token'ın `aud` claim'i `eshop-api` taşır ve doğrulanır. Bu, aynı realm'de
    başka bir client için kesilmiş token'ların kabul edilmesini engeller (audience confusion).
  - `RequireHttpsMetadata = false` — yalnız dev (Keycloak compose ağında http). Prod'da TLS gerekir.
- **Rol akışı:** Keycloak rolleri `realm_access.roles` altında gelir; `MapRealmRoles` olayı
  bunları standart `ClaimTypes.Role` claim'lerine düzleştirir, böylece
  `RequireRole(...)` ve `[Authorize(Roles=...)]` çalışır.
- **Kimlik propagasyonu:** Kullanıcı kimliği **daima token'dan** (`sub`) alınır, istemci
  gövdesinden değil. Basket/Users/Order müşteri uçları `user.GetUserId()` ile çalışır;
  checkout'ta `UserName`/`CustomerId` token `sub`'undan set edilir (istemci ne gönderirse ezilir).

> **Konfig nerede:** `Jwt:Authority` + `Jwt:Audience` host-dev için `appsettings.json`'da
> (Catalog/Users/Payment), dockerize çalıştırmada `docker-compose.override.yml` env'inde
> (`Jwt__Authority`, `Jwt__Audience`) — Catalog, Basket, Order, Users, Payment ve Gateway.

---

## 2. Roller

Realm export (`keycloak/realms/eshop-realm.json`):

| Rol | Anlamı |
|---|---|
| `customer` | Kayıtlı alışverişçi (kendi sepeti/siparişi/profili) |
| `support-agent` | İade ve müşteri siparişlerini görür/yönetir |
| `fulfillment-manager` | Sipariş durumunu ilerletir (process/ship/deliver) |
| `catalog-manager` | Ürün/kupon yönetimi |
| `super-admin` | Tam yönetim (kullanıcı/rol dahil) |

Seed kullanıcılar: `customer1` (customer), `admin1` (tüm roller).

---

## 3. Yetkilendirme (Authorization) — uç bazında

Yetki **her serviste, uç bazında** uygulanır (gateway tek başına yeterli değildir; servisler
doğrudan portlarından da erişilebilir). Aşağıdaki tablo mevcut durumu (FIX-026/027 sonrası) gösterir.

### Catalog (`:6000`)
| Uç | Politika |
|---|---|
| `GET /products*` (browse) | Anonim (public okuma) |
| `POST /product-create`, `PUT /product-update`, `DELETE /product-delete/{id}` | `catalog-manager`, `super-admin` |

### Basket (`:6001`)
| Uç | Politika |
|---|---|
| `GET /basket`, `POST /basket-store`, `DELETE /basket/{}`, `POST /basket/checkout` | Authenticated (kimlik = token `sub`) |

### Users (`:6004`)
| Uç | Politika |
|---|---|
| `GET/PUT /users/me/profile`, adresler, favoriler | Authenticated; yalnız kendi verisi (`sub`) |

### Order (`:6003`) — **FIX-026**
| Uç | Politika |
|---|---|
| `GET /me/orders` | Authenticated (yalnız kendi siparişleri, `CustomerId == sub`) |
| `POST /me/orders/{id}/returns` | Authenticated (sahiplik handler'da doğrulanır) |
| `GET /orders` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `GET /orders/by-customer/{customerId}` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `GET /orders/by-name/{orderName}` | `fulfillment-manager`, `support-agent`, `super-admin` |
| `PUT /orders` | `fulfillment-manager`, `super-admin` |
| `DELETE /orders/{id}` | `super-admin` |
| `POST /orders/{id}/confirm\|process\|ship\|deliver\|cancel` | `fulfillment-manager`, `super-admin` |
| `POST /orders/{id}/returns/approve\|reject\|refund` | `support-agent`, `super-admin` |
| ~~`POST /orders`~~ | **Kaldırıldı** — sipariş yalnız checkout saga consumer'ı (`BasketCheckoutEventHandler`) ile oluşturulur |

### Payment (`:6005`) — **FIX-027**
| Uç | Politika |
|---|---|
| `GET /payments/{orderId}` | `support-agent`, `super-admin` |

> **FIX-027 öncesi** PaymentAPI'de auth hiç kurulu değildi; artık `AddStandardJwtAuth` +
> `UseAuthentication/UseAuthorization` pipeline'da ve uç role kilitli.

---

## 4. Gateway Edge Güvenliği

[`YarpApiGateway/Program.cs`](../../Src/ApiGateways/YarpApiGateway/Program.cs) +
`appsettings.json` (Routes):

- **Edge authentication:** `basket`, `ordering`, `users` route'larında `AuthorizationPolicy: default`
  (kimliği doğrulanmış kullanıcı şart). `catalog` route'u public (okuma).
- **Rate limiting (FIX-030):** İstemci-başı **partition**lı limiter — anahtar: kimlik
  doğrulanmışsa token `sub`, değilse istemci IP. Böylece tek bir kötü niyetli istemci ortak
  kovayı doldurup herkesi kilitleyemez. Limit aşımında `429` + `Retry-After`.

  | Politika | Uygulanan route | Pencere / Limit |
  |---|---|---|
  | `fixed` (sıkı) | `ordering-route` | 10sn / 5 |
  | `auth-sensitive` (orta) | `basket-route`, `users-route` | 10sn / 20 |
  | `catalog-loose` (gevşek) | `catalog-route` | 10sn / 100 |

- **Gerçek istemci IP'si:** `UseForwardedHeaders()` (X-Forwarded-For) ile proxy arkasında
  gerçek IP partition'a yansır. **Prod uyarısı:** `KnownProxies/KnownNetworks` güvenilen LB
  aralığıyla kısıtlanmalı, aksi halde X-Forwarded-For spoof'lanıp partition atlatılabilir.

> Servisler kendi portlarından (6000–6006) doğrudan da erişilebilir; bu nedenle yetki
> **servis içinde** zorunludur (gateway bypass'a karşı). Prod'da servis portları dışa açılmamalı.

---

## 5. Checkout Idempotency (FIX-028)

`POST /basket/checkout` artık opsiyonel bir **`Idempotency-Key`** HTTP başlığı kabul eder.

- Akış: [`CheckoutBasketEndpoints.cs`](../../Src/Services/Basket/BasketAPI/Basket/CheckoutBasket/CheckoutBasketEndpoints.cs)
  başlığı okur → [`CheckoutBasketHandler`](../../Src/Services/Basket/BasketAPI/Basket/CheckoutBasket/CheckoutBasketHandler.cs)
  anahtarı kontrol eder.
- Anahtar daha önce işlendiyse, ilk sonucu **replay** eder (yeni checkout/ödeme başlatmaz).
  Aksi halde checkout'u başlatır ve anahtarı [`IdempotencyRecord`](../../Src/Services/Basket/BasketAPI/Models/IdempotencyRecord.cs)
  (Marten dokümanı, key = Idempotency-Key) olarak **sepet + outbox ile aynı transaction'da** yazar.
- Anahtar yoksa eski davranış korunur (geriye dönük uyumlu). Mevcut `CheckoutPending` koruması
  ardışık tekrarları yine engeller.
- **Mesaj düzeyi idempotency** ayrıca mevcuttur: `PaymentCaptureConsumer` ve order oluşturma
  `CheckoutId` üzerinde deterministiktir → RabbitMQ tekrar teslimi çift ödeme/sipariş yaratmaz.
- **Erteleme notu:** Salt-eşzamanlı, anahtarsız checkout yarışı için tam Marten optimistic
  concurrency, tüm `ShoppingCard` yazımlarını etkileyeceğinden ayrı bir takip kalemine bırakıldı.

---

## 6. Ödeme Verisi İşleme

- Gerçek ödeme sağlayıcısı yok (PaymentAPI mock). Kart **tam numarası ve CVV saklanmaz**.
- Checkout DTO'su tokenize alanlar taşır: `PaymentToken`, `PaymentReference`, `CardLast4` (yalnız
  son 4, validator regex'i ile), `CardBrand`. Order tarafında `PaymentDataSanitizer`
  token'ı normalize eder ve CVV'yi redakte eder.
- `PaymentRecord` yalnız `CustomerId`, `Amount`, `Status` tutar; uç role kilitlidir (Bölüm 3).

---

## 7. Sırların Yönetimi

- DB/broker/Keycloak kimlik bilgileri `.env` (gitignore'da) + `docker-compose` env üzerinden gelir;
  repo'da gerçek sır yoktur (`.env.example` yalnız örnek/dev varsayılanları).
- Dev varsayılanları (postgres/postgres, guest/guest, admin/admin) **yalnız geliştirme** içindir;
  prod'da secret store/rotasyon gerekir.

---

## 8. Açık / Prod Öncesi Sertleştirme

- HTTPS redirection / HSTS (FIX-014) — henüz yok; prod'da gateway'de TLS sonlandırma.
- `AllowedHosts: *`, `RequireHttpsMetadata=false` — dev kabul, prod'da daraltılmalı.
- Servis portlarının doğrudan ifşası — prod'da yalnız gateway dışa açık olmalı.
- **FIX-029 runtime doğrulaması:** audience doğrulaması fail-closed'dur; Keycloak çalışırken
  token alınıp `aud=eshop-api` ve servislerin token'ı kabul ettiği **merge öncesi doğrulanmalıdır**.

Devamı/ilgili: [08 — Gateway & Dağıtım](08-gateway-and-deployment.md) · [07 — Checkout Akışı](07-checkout-flow.md)
