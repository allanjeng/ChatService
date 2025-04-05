# Chat Service

A real-time chat application built with ASP.NET Core, SignalR, and SQLite.

## Features

- Real-time messaging using SignalR
- User authentication and registration
- Message history with pagination
- SQLite database for data persistence
- Swagger API documentation
- Modern, responsive web interface

## Prerequisites

- .NET 8.0 SDK or later
- A modern web browser

## Getting Started

1. Clone the repository
2. Navigate to the project directory
3. Run the following commands:

```bash
dotnet restore
dotnet run
```

4. Open your browser and navigate to `https://localhost:5001` (or the URL shown in the console)

## API Endpoints

### Authentication

- `POST /api/auth/login`
  - Authenticates a user or creates a new one if not found
  - Request body: username (plain text)
  - Returns: `{ userId: number, username: string }`

### Messages

- `GET /messages`
  - Retrieves the last 50 messages
  - Returns: Array of messages with user information

### WebSocket (SignalR)

- Hub URL: `/chathub`
- Methods:
  - `SendMessage(message: string, userId: number)`
  - Events:
    - `ReceiveMessage(username: string, message: string)`

## Project Structure

- `Models/` - Data models (User, Message)
- `Services/` - Business logic services
- `Hubs/` - SignalR hub implementations
- `Data/` - Database context and migrations
- `wwwroot/` - Static files and frontend assets

## Development

The project uses:
- ASP.NET Core 8.0
- SignalR for real-time communication
- Entity Framework Core with SQLite
- Swagger for API documentation

## License

MIT