# BonusSystem Development Guide

## Build Commands
- `dotnet build BonusSystem.sln` - Build the entire solution
- `dotnet restore` - Restore dependencies
- `docker-compose up -d` - Start the Docker environment with API, PostgreSQL and pgAdmin
- `docker-compose build api` - Rebuild the API container

## Naming Conventions
- **Interfaces**: Prefix with "I" (e.g., `IUserRepository`)
- **DTOs**: Suffix with "Dto" (e.g., `UserDto`, `TransactionDto`)
- **Entities**: Suffix with "Entity" (e.g., `UserEntity`)
- **Services**: Suffix with "Service" (e.g., `BuyerBffService`)
- **PascalCase**: For class names, methods, properties
- **camelCase**: For local variables and parameters

## Code Organization
- **Vertical Slice Architecture**: Features organized by user role
- **BFF (Backend for Frontend)**: Role-specific API interfaces
- **Repositories**: Data access abstraction layer
- **Services**: Business logic implementation
- **Unit Tests**: Organized by component (API, Core, Infrastructure)
- **XML Documentation**: Required for public APIs and service implementations

## Error Handling
- **Consistent Pattern**: Try/catch blocks with specific exceptions
- **Logging**: Use ILogger<T> for structured logging
- **Response Handling**: Return appropriate HTTP status codes
- **Domain Exceptions**: KeyNotFoundException, UnauthorizedAccessException, InvalidOperationException
- **Error Propagation**: Log at source, bubble up domain exceptions