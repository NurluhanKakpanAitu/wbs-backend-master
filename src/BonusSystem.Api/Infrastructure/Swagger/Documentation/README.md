# Swagger Documentation Guide

This directory contains example providers for the Swagger documentation for the BonusSystem API.

## Implemented Example Response Providers

The following example providers have been implemented:

1. **AuthExamples**: Provides example responses for authentication-related endpoints, including:
   - User registration (both buyer-specific and general)
   - User login
   - User context retrieval

2. **TransactionExamples**: Provides example responses for transaction-related endpoints, including:
   - Processing a transaction
   - Getting transaction history
   - Getting bonus balance
   - Confirming transaction returns

3. **CompanyStoreExamples**: Provides example responses for company and store-related endpoints, including:
   - Finding stores
   - Getting store transactions
   - Getting store balance

4. **AdminExamples**: Provides example responses for admin-related endpoints, including:
   - Registering a company
   - Updating company status
   - Moderating stores
   - Crediting company balance
   - Getting system transactions
   - Sending system notifications

5. **ObserverExamples**: Provides example responses for observer-related endpoints, including:
   - Getting system statistics
   - Getting transaction summary
   - Getting companies overview

6. **BuyerSpecialtyExamples**: Provides example responses for unique buyer features, including:
   - Generating QR codes
   - Finding nearby stores
   - Canceling transactions

## How to View the Examples

1. Start the BonusSystem API project
2. Navigate to the Swagger UI at `/api-docs`
3. Select any endpoint with a "200" response
4. Expand the "Response body" section to view the example response

## How to Add More Examples

To add more example providers:

1. Create a new class in this directory that implements `IOperationFilter`
2. Implement the `Apply` method to add examples to the appropriate operations
3. Register the filter in `ApiExtensions.cs` using `c.OperationFilter<Documentation.YourExampleClass>()`

## Example Format

Example responses are provided using OpenAPI objects:

```csharp
var example = new OpenApiObject
{
    ["propertyName"] = new OpenApiString("example value"),
    ["numericProperty"] = new OpenApiDouble(123.45),
    ["boolProperty"] = new OpenApiBoolean(true)
};

// Add to response
successResponse.Content["application/json"].Example = example;
```

## Common Response Types

The system uses consistent response formats across endpoints:

- **GET requests**: Return either a single object or array of objects
- **POST/PUT requests**: Return a confirmation with relevant IDs and status
- **Error responses**: Contain a message property with the error description

## Response Schema

The response schema for all operations is defined using the schema filters and enhancements in `SchemaEnhancementFilter.cs`, which provides descriptions and other metadata for commonly used DTOs.