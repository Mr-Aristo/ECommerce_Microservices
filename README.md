# E-Commerce Microservices

This repository contains a modular e-commerce backend built with `.NET 9`, organized with microservices and supporting infrastructure.  
The solution demonstrates:

- service-level data ownership
- synchronous communication (HTTP/gRPC)
- asynchronous communication (RabbitMQ via MassTransit)
- resilient checkout orchestration with `Outbox + Saga`
- CQRS-style handlers with MediatR

## Architecture Diagrams
### Whole Project
![BigPicture](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/Big%20picture.png)

### Catalog Service
![Catalog](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/Catalog.png)

### Ordering Service
![Ordering](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/ordering.png)

### Basket Service
![Basket](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/Basket.png)

### Discount Service
![Discount](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/Discount.png)

### API Gateway
![ApiGateway](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/APi%20GetWay.png)

### Web Client
![Client](https://github.com/Mr-Aristo/ECommerce_Microservices/blob/master/ProjectArchitecture/WEbClient.png)

## Solution Structure

```text
Src
  ApiGateways
    YarpApiGateway
  BuildingBlocks
    BuildingBlock
    BuildingBlockMessaging
  Services
    CatalogAPI
    Basket/BasketAPI
    DiscountGrpc
    Order
      Order.API
      Order.Application
      Order.Domain
      Order.Infrastructure
Tests
  ECommerce_Tests
```

## Services Overview

| Service | Main Responsibility | Storage / Integration | Docker HTTP Port |
|---|---|---|---|
| CatalogAPI | Product CRUD and product browsing | PostgreSQL (Marten) | `6000` |
| BasketAPI | Basket CRUD, discount-aware basket pricing, checkout start | PostgreSQL (Marten), Redis, gRPC client, RabbitMQ | `6001` |
| DiscountGrpc | Coupon management via gRPC | SQLite (EF Core) | `6002` |
| Order.API | Order CRUD, checkout order creation from events | SQL Server (EF Core), RabbitMQ | `6003` |
| YarpApiGateway | Reverse proxy + basic rate limiting | Proxies to services | Local profile: `5004` |

## Core Technologies

- `.NET 9`
- `Minimal API` + `Carter`
- `MediatR` (commands/queries + pipeline behaviors)
- `FluentValidation`
- `Mapster`
- `Marten` for document persistence in Catalog/Basket
- `Entity Framework Core` in Order and Discount services
- `MassTransit` + `RabbitMQ`
- `Redis` distributed cache
- `YARP` API Gateway

## Checkout Flow (Outbox + Saga)

Checkout is eventually consistent and resilient to temporary broker/network failures.

1. Client calls `POST /basket/checkout` in Basket API.
2. Basket API validates basket state (`Active`, non-empty).
3. Basket is marked as `CheckoutPending` and a `BasketCheckoutOutboxMessage` is stored in Basket DB in the same transaction.
4. `BasketCheckoutOutboxDispatcher` background service reads pending outbox rows and publishes `BasketCheckoutEvent` to RabbitMQ.
5. Order service consumes the event, maps it to `CreateOrderCommand`, and creates the order.
6. Order service publishes `BasketCheckoutSucceededEvent` when order creation succeeds.
7. Order service publishes `BasketCheckoutFailedEvent` when processing fails.
8. Basket service consumes the result event and finalizes state (`success -> delete basket`, `failure -> set basket back to Active`).

This design avoids losing checkout requests even if broker publishing fails immediately after API request handling.

## API Gateway Routing

When running `YarpApiGateway`, use:

- `/catalog-service/{**catch-all}` -> `http://localhost:6000/`
- `/basket-service/{**catch-all}` -> `http://localhost:6001/`
- `/ordering-service/{**catch-all}` -> `http://localhost:6003/`

`ordering-service` route has a fixed window rate limiter (`5 requests / 10 seconds`).

## Key Endpoints

### Catalog API (`http://localhost:6000`)
- `GET /products`
- `GET /products/{id}`
- `GET /products/category/{category}`
- `POST /product-create`
- `PUT /product-update`
- `DELETE /product-delete/{id}`

### Basket API (`http://localhost:6001`)
- `GET /basket/{userName}`
- `POST /basket-store`
- `POST /basket/checkout`
- `DELETE /basket/{userName}`

### Order API (`http://localhost:6003`)
- `GET /orders`
- `GET /orders/by-name/{orderName}`
- `GET /orders/by-customer/{customerId}`
- `POST /orders`
- `PUT /orders`
- `DELETE /orders/{id}`

### Health Checks
- Catalog: `GET /health`
- Basket: `GET /health`
- Order: `GET /health`

## Running The Project

### Prerequisites
- `.NET SDK 9.0`
- `Docker Desktop`
- `Visual Studio 2022/2026` or `VS Code`

### Option A: Run With Docker Compose (Recommended)

From repository root:

```bash
docker compose up -d --build
```

This starts:
- PostgreSQL (`catalogdb`, `basketdb`)
- SQL Server (`orderdb`)
- Redis (`distributedcache`)
- RabbitMQ (`messagebroker`, management UI on `15672`)
- Catalog, Basket, Discount gRPC, and Order services

Note: `YarpApiGateway` is not part of `docker-compose.yml` and can be run separately from Visual Studio or CLI if needed.

RabbitMQ default credentials:

- Username: `guest`
- Password: `guest`

### Option B: Run Services From Visual Studio / CLI

If you want to debug services individually:

1. Start infrastructure containers first (db/cache/broker).
2. Run service projects from solution startup profile or `dotnet run`.

Default local HTTP ports from launch profiles:

- CatalogAPI: `5000`
- BasketAPI: `5001`
- DiscountGrpc: `5002`
- Order.API: `5003`
- YarpApiGateway: `5004`

## Quick Test Scenario

1. Fetch products from Catalog:
   `GET /products`
2. Create or update a basket in Basket API:
   `POST /basket-store`
3. Trigger checkout:
   `POST /basket/checkout`
4. Verify order creation in Order API:
   `GET /orders/by-customer/{customerId}`

Note: checkout creates orders asynchronously through messaging.  
`POST /orders` is also available for direct order creation.

## Running Tests

```bash
dotnet test
```

## License

This project is licensed under the terms defined in `LICENSE.txt`.
