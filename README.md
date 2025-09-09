# BonusSystem Prototype

A bonus tracking and management platform designed to provide a flexible, scalable solution for managing bonus transactions across multiple user roles.

## Project Overview

BonusSystem is a comprehensive platform that allows different user roles (buyers, sellers, admins, etc.) to interact with a bonus point system. It follows a Vertical Slice Monolith with BFF (Backend for Frontend) architecture using .NET 9 with PostgreSQL as the backend database.

## Architecture

- **Architecture Style**: Monolithic with Modular Design
- **API Pattern**: Backend for Frontend (BFF) with Vertical Slices
- **Technology Stack**:
  - Backend: .NET 9, PostgreSQL
  - Frontend: React, Material UI
  - Authentication: JWT Authentication

## User Roles

1. **Buyers**: End users who earn and spend bonuses
   - View bonus balance
   - Make purchases and earn/spend points
   - View transaction history
   - Generate QR code for in-store use
   - Find participating stores

2. **Sellers**: Store employees who process transactions
   - Scan buyer QR codes
   - Process transactions (earn/spend points)
   - View store transaction history
   - Handle purchase returns

3. **Admins**: System administrators
   - Manage companies and stores
   - Process store approval requests
   - Monitor system usage
   - Manage bonus crediting
   - Send system notifications

4. **Observers**: Company or system observers
   - View statistics and reports
   - Monitor bonus distribution
   - Analyze transaction patterns
   - No modification privileges

## Project Structure

- **BonusSystem.Api**: Web API with BFF implementation
- **BonusSystem.Core**: Domain models and business logic
- **BonusSystem.Infrastructure**: Data access and external services integration
- **BonusSystem.Shared**: DTOs and shared utilities
- **client**: React frontend application

## Development Environment Setup

### Prerequisites

- Docker and Docker Compose
- .NET 9 SDK (for local development)
- Node.js and npm (for frontend development)

### Getting Started with Docker

1. Clone the repository:

```bash
git clone https://github.com/danila-permogorskii/bonus-system.git
cd bonus-system
```

2. Start the Docker environment:

```bash
docker-compose up -d
```

This will start the following services:

- **API**: .NET 9 API (http://localhost:5001)
- **Frontend**: React UI (http://localhost:3000)
- **PostgreSQL**: Database (localhost:5432)
- **pgAdmin**: Database management tool (http://localhost:5050)

3. Access the services:

- **Frontend**: http://localhost:3000
- **API Documentation**: http://localhost:5001/api-docs
- **pgAdmin**: http://localhost:5050 (Email: admin@bonussystem.com, Password: admin)

### Demo Accounts

For demonstration purposes, the following test accounts are available:

| Role | Email | Password |
|------|-------|----------|
| Buyer | buyer1@example.com | Password123! |
| Seller | seller1@example.com | Password123! |
| Admin | admin1@example.com | Password123! |
| Observer | observer1@example.com | Password123! |

### Environment Variables

All configuration is stored in the `.env` file at the root of the project. You can modify these values to change ports, credentials, etc.

### Port Configuration

The project uses standardized ports for all services:

| Service          | Port  |
|------------------|----------|
| BonusSystem API  | 5001     |
| Frontend UI      | 3000     |
| PostgreSQL       | 5432     |
| pgAdmin          | 5050     |

### Stopping the Environment

```bash
docker-compose down
```

To remove volumes as well:

```bash
docker-compose down -v
```

## Development Guidelines

1. Follow the Vertical Slice architecture pattern
2. Implement role-specific endpoints in their respective feature folders
3. Use BFF services to implement business logic
4. Maintain separation of concerns between layers

## Notes on HTTPS

- The Docker development environment uses HTTP only for simplicity
- When running locally with `dotnet run`, HTTPS is available
- For production deployment, proper HTTPS configuration should be implemented

## License

This project is for demonstration purposes only.
