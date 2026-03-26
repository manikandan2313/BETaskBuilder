# TaskBoard API

A production-style ASP.NET Core Web API for a Task Board application built with Clean Architecture principles.

## Features

- **Clean Architecture**: Controller → Service → Repository pattern
- **Entity Framework Core** with SQL Server
- **Async/Await** throughout the application
- **Concurrency Handling** using RowVersion
- **Soft Delete** functionality
- **Activity Logging** for all operations
- **Status Transition Validation**
- **Pagination and Filtering**
- **Global Exception Handling**
- **DTOs** with AutoMapper
- **Comprehensive Logging**

## Architecture

### Domain Layer
- Entities: `TaskItem`, `TaskActivity`
- Enums: `TaskStatus`, `TaskPriority`, `TaskActivityAction`

### Infrastructure Layer
- `AppDbContext` with EF Core configuration
- Repository implementations
- Database migrations

### Application Layer
- Service layer with business logic
- DTOs and AutoMapper profiles
- API response models

### API Layer
- Controllers with RESTful endpoints
- Global exception handling middleware
- Swagger/OpenAPI documentation

## API Endpoints

### Task Items

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/taskitems` | Get all tasks with optional filtering and pagination |
| GET | `/api/taskitems/{id}` | Get a specific task with activities |
| POST | `/api/taskitems` | Create a new task |
| PUT | `/api/taskitems/{id}` | Update a task (full update) |
| PATCH | `/api/taskitems/{id}/status` | Update task status only |
| DELETE | `/api/taskitems/{id}` | Soft delete a task |

### Query Parameters

- `status`: Filter by task status (`Todo`, `InProgress`, `Done`)
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)

### Headers for Concurrency

- `If-Match`: Base64 encoded RowVersion for optimistic concurrency

## Status Transitions

Valid status transitions:
- Todo → InProgress
- InProgress → Done
- Done → InProgress (reopening)
- InProgress → Todo (moving back)
- Done → Todo (reopening)

## API Response Format

```json
{
  "success": true,
  "data": {},
  "message": "",
  "errors": []
}
```

## Getting Started

### Prerequisites

- .NET 7.0 or later
- SQL Server (or SQL Server LocalDB)

### Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=TaskBoardDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Running the Application

1. Navigate to the API project directory:
   ```bash
   cd src/TaskBoard.API
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to `https://localhost:7000` to access Swagger UI.

### Database Migration

The application automatically applies migrations on startup. The database will be created with the required tables.

## Sample Requests

### Create a Task

```http
POST /api/taskitems
Content-Type: application/json

{
  "title": "Sample Task",
  "description": "This is a sample task description",
  "priority": "Medium"
}
```

### Get Tasks with Filtering

```http
GET /api/taskitems?status=Todo&page=1&pageSize=10
```

### Update Task Status

```http
PATCH /api/taskitems/1/status
Content-Type: application/json
If-Match: "AAAAAAAAB9s="

{
  "status": "InProgress"
}
```

### Update Task

```http
PUT /api/taskitems/1
Content-Type: application/json
If-Match: "AAAAAAAAB9s="

{
  "title": "Updated Task Title",
  "description": "Updated description",
  "priority": "High"
}
```

### Delete Task

```http
DELETE /api/taskitems/1
```

## Error Handling

The API includes comprehensive error handling:

- **400 Bad Request**: Validation errors, invalid status transitions
- **404 Not Found**: Resource not found
- **412 Precondition Failed**: Concurrency conflicts
- **500 Internal Server Error**: Unexpected server errors

All errors follow the consistent API response format.

## Concurrency Handling

The API implements optimistic concurrency using RowVersion:

1. Include the `If-Match` header with the RowVersion value when updating
2. The server validates the RowVersion before applying changes
3. If a conflict occurs, the API returns a 412 status with an appropriate error message

## Activity Logging

All significant operations are logged:
- Task creation
- Task updates
- Status changes
- Soft deletes

Activities can be retrieved when getting a specific task.

## Development

### Project Structure

```
TaskBoard/
├── src/
│   ├── TaskBoard.API/           # Web API project
│   ├── TaskBoard.Application/   # Application layer
│   ├── TaskBoard.Domain/        # Domain entities
│   └── TaskBoard.Infrastructure/ # Data access
├── README.md
└── TaskBoard.sln
```

### Adding New Features

1. Add entities to the Domain layer
2. Update the DbContext and create migrations
3. Add repository interfaces and implementations
4. Create service layer methods
5. Add DTOs and AutoMapper mappings
6. Implement controller actions

## License

This project is provided as-is for educational and development purposes.
