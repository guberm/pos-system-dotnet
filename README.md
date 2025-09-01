# POS System Backend API

A modern Point of Sale (POS) system backend built with .NET 9, demonstrating professional-grade software development skills including C#, Entity Framework Core, SQL Server, XML configuration, and RESTful API design.

## Features

- **Product Management**: CRUD operations for products with categories and inventory tracking
- **Order Processing**: Complete order lifecycle management with payment processing
- **Database Integration**: Entity Framework Core with SQL Server LocalDB
- **XML Configuration**: Advanced XML-based configuration management with validation
- **RESTful API**: Well-designed REST endpoints with proper HTTP status codes
- **Data Validation**: Comprehensive input validation and error handling
- **Business Logic**: Transaction management, stock control, and pricing calculations
- **Documentation**: Swagger/OpenAPI integration for API documentation
- **Logging**: Structured logging throughout the application
- **Health Checks**: Database connectivity monitoring

## Technology Stack

- **.NET 9.0** - Modern C# framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - Object-relational mapping
- **SQL Server LocalDB** - Development database
- **Swagger/OpenAPI** - API documentation
- **XML Processing** - Configuration management
- **Dependency Injection** - Service registration and lifecycle management

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- SQL Server LocalDB (included with Visual Studio)

## Getting Started

### 1. Clone and Setup
```bash
# Navigate to project directory
cd POSSystem

# Restore dependencies
dotnet restore

# Build the project
dotnet build
```

### 2. Database Setup
The application uses SQL Server LocalDB and will automatically create the database on first run with sample data.

### 3. Run the Application
```bash
dotnet run
```

The API will be available at:
- **Swagger UI**: `https://localhost:7065` (default)
- **Health Check**: `https://localhost:7065/health`

## Code Highlights

### Advanced Entity Framework Usage
- Complex relationships with proper foreign keys
- Fluent API configuration for database schema
- Repository pattern with service layer abstraction
- Transaction management for data consistency
- Seed data initialization

### XML Configuration System
- Custom XML configuration parser
- Validation and error handling
- Caching and performance optimization
- Type-safe configuration access
- Support for complex nested structures

### Professional API Design
- RESTful endpoint design following best practices
- Proper HTTP status codes and error responses
- Input validation with model binding
- Pagination and filtering support
- Consistent DTO patterns

### Business Logic Implementation
- Order processing with inventory management
- Payment processing simulation
- Automatic tax and total calculations
- Stock level tracking and validation
- Order status workflow management

## API Endpoints

### Products API
- `GET /api/products` - Get all products with filtering and pagination
- `GET /api/products/{id}` - Get specific product
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Soft delete product
- `GET /api/products/low-stock` - Get low stock products

### Orders API
- `POST /api/orders` - Create new order
- `GET /api/orders/{id}` - Get specific order
- `GET /api/orders` - Get orders by date range
- `PATCH /api/orders/{id}/status` - Update order status
- `POST /api/orders/{id}/payment` - Process payment
- `GET /api/orders/{id}/total` - Calculate order total

### System Endpoints
- `GET /health` - Health check with database connectivity

## Project Structure

```
POSSystem/
├── Controllers/           # API Controllers
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Models/               # Domain Models
│   ├── Product.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Customer.cs
├── Data/                 # Data Layer
│   └── POSContext.cs
├── Services/             # Business Logic
│   ├── IOrderService.cs
│   └── OrderService.cs
├── Configuration/        # XML Configuration
│   ├── ConfigurationModels.cs
│   ├── XmlConfigurationService.cs
│   └── pos-config.xml
└── Program.cs           # Application Startup
```

## Sample Data

The application includes seeded data:
- **Categories**: Electronics, Clothing, Food & Beverages
- **Products**: Sample products with stock quantities
- **Configuration**: Complete XML configuration example

## Configuration

The system uses XML-based configuration for:
- Tax rates and currency settings
- Payment method configuration
- Security policies
- Reporting settings
- Inventory thresholds

Example configuration access:
```csharp
var taxRate = configService.GetSetting<decimal>("general.taxrate", 0.08m);
```

## Technical Decisions

### Why Entity Framework Core?
- Provides robust ORM capabilities with excellent performance
- Code-first approach for database schema management
- Built-in support for migrations and seeding
- Strong typing and LINQ query support

### Why XML Configuration?
- Demonstrates XML parsing and manipulation skills
- Provides hierarchical configuration structure
- Supports validation and schema enforcement
- Easily readable and maintainable by non-developers

### Why Service Layer Pattern?
- Separates business logic from API controllers
- Enables easier unit testing and mocking
- Promotes code reusability and maintainability
- Supports dependency injection patterns

## Production Considerations

For production deployment, consider:
- **Database**: Migrate to full SQL Server instance
- **Authentication**: Add JWT or OAuth2 authentication
- **Caching**: Implement Redis for performance
- **Monitoring**: Add Application Insights or similar
- **Security**: Enable HTTPS, add rate limiting
- **Logging**: Configure structured logging to external systems

## Developer Notes

This project demonstrates:
- **Clean Architecture**: Separation of concerns with proper layering
- **SOLID Principles**: Dependency inversion, single responsibility
- **Error Handling**: Comprehensive exception management
- **Performance**: Async/await patterns, efficient queries
- **Maintainability**: Clear code structure and documentation
- **Testability**: Service abstractions and dependency injection

## License

This project is created as a demonstration of backend development skills for evaluation purposes.

---

**Built with care using .NET 9 and modern C# practices**
