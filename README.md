# Chat Service

A real-time chat application built with ASP.NET Core 8.0, SignalR, and Entity Framework Core. This service provides a robust backend for chat functionality with features like message caching, authentication, and real-time communication.

## Features

- Real-time chat using SignalR
- Message caching for improved performance
- SQLite database for message persistence
- Swagger/OpenAPI documentation
- RESTful API endpoints
- Authentication service
- Message generation service

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or Visual Studio Code
- Git (for cloning the repository)

## Getting Started

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd ChatService
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run --project ChatService
   ```

The application will start and be available at:
- Web API: https://localhost:7074
- Swagger UI: https://localhost:7074/swagger
- SignalR Hub: https://localhost:7074/chathub

## Testing

The solution includes a test project that can be run using:

```bash
dotnet test
```

## Project Structure

- `ChatService/` - Main web application project
  - `Controllers/` - API controllers
  - `Data/` - Database context and models
  - `Hubs/` - SignalR hub implementations
  - `Services/` - Business logic services
  - `Configuration/` - Application configuration
- `ChatService.Tests/` - Unit tests project

## API Documentation

Once the application is running, you can access the Swagger documentation at:
```
https://localhost:7074/swagger
```

This provides interactive documentation for all API endpoints.

## SignalR Hub

The chat hub is available at `/chathub` and supports the following operations:
- Send messages
- Join chat rooms
- Receive real-time updates

## Database

The application uses SQLite for data persistence. The database file (`chat.db`) will be automatically created in the project directory when the application first runs.

### Why SQLite?

I chose SQLite for this project for several key reasons:

1. **Simplicity and Zero Configuration**
   - No separate server process required
   - Single file database makes it easy to deploy and backup
   - Perfect for development and testing environments

2. **Lightweight and Fast**
   - Minimal resource requirements
   - Excellent performance for small to medium-sized applications
   - Ideal for a chat service where the data volume is manageable

3. **Development Experience**
   - Easy to set up and get started
   - Great for rapid prototyping and development
   - Simplifies the development workflow with no external dependencies

4. **Interview Project Considerations**
   - Demonstrates understanding of Entity Framework Core
   - Shows ability to work with different database providers
   - Makes it easy for interviewers to run and test the application

5. **Cross-Platform Compatibility**
   - Works seamlessly across different operating systems
   - No additional setup required for different environments
   - Consistent behavior across development and production

### Production Considerations

In a production environment, I would make the following changes:

1. **Database Choice**
   - Switch to PostgreSQL for its superior performance, reliability, and advanced features
   - PostgreSQL's native support for JSON and full-text search would be valuable for chat applications
   - Better handling of concurrent connections and write operations

2. **Caching Strategy**
   - Implement a multi-layer caching approach:
     - **In-Memory Cache** (First Level)
       - Cache frequently accessed user data and recent messages
       - Use sliding expiration for active conversations
       - Implement cache invalidation on message updates
     - **Distributed Cache** (Second Level)
       - Use Redis for shared cache across multiple server instances
       - Store user sessions and connection states
       - Implement pub/sub for real-time cache invalidation
     - **Database Cache** (Third Level)
       - Use PostgreSQL's built-in caching
       - Implement materialized views for complex queries
       - Use database partitioning for message history

3. **Scalability**
   - Implement database sharding for message history
   - Use read replicas for scaling read operations
   - Implement connection pooling and connection management
   - Consider using a message queue (like RabbitMQ) for handling high message volumes

4. **Monitoring and Maintenance**
   - Set up database monitoring and alerting
   - Implement automated backup strategies
   - Use database migration tools for schema changes
   - Implement connection health checks and failover mechanisms

This multi-layered approach would ensure high performance, reliability, and scalability for a production chat service.

## Configuration

The application uses the following configuration settings (found in `appsettings.json`):
- Cache settings for message caching
- Database connection string
- SignalR hub configuration

## Development

To modify the application:
1. Make your changes in the appropriate project
2. Run the tests to ensure functionality
3. Build and run the application to test your changes

## Troubleshooting

If you encounter any issues:
1. Ensure all prerequisites are installed
2. Check the application logs for error messages
3. Verify the database file is accessible
4. Ensure the required ports (7074 for HTTPS, 5193 for HTTP) are not in use by other applications