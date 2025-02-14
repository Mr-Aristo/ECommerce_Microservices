# Building Block Library

## Purpose
The **Building Block Library** is designed to provide an abstraction layer over the **MediatR** library to simplify the implementation of the **CQRS (Command Query Responsibility Segregation)** pattern. This library contains reusable logic to support consistent and efficient communication across multiple services.

## Key Features
- **CQRS Abstraction**: Simplifies command and query handling using MediatR.
- **Reusable Logic**: Centralizes shared functionality for improved maintainability.
- **Service-Oriented Design**: Supports catalog, basket, and order services.

## Usage
This library is utilized by the following services:
- **Catalog Service**
- **Basket Service**
- **Order Service**

By leveraging this library, these services benefit from reduced code duplication, improved consistency, and streamlined CQRS implementation.

