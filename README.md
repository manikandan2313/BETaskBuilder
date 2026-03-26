# TaskBoard API

A production-style ASP.NET Core Web API for a Task Board application built with Clean Architecture principles.

## Setup Instructions

### Prerequisites

- **.NET 7.0 SDK** or later
- **SQL Server** (SQL Server Express, LocalDB, or full SQL Server)
- **Visual Studio 2022** or **VS Code** with .NET extensions

### Database Setup

1. **Install SQL Server** if not already installed
2. **Create a database** (optional - the app will create it automatically)
3. **Update connection string** in `src/TaskBoard.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TaskBoardDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Running the Application

#### Option 1: Using Visual Studio
1. Open `TaskBoard.sln` in Visual Studio
2. Set `TaskBoard.API` as the startup project
3. Press `F5` or click "Start Debugging"

#### Option 2: Using Command Line
1. Navigate to the API project directory:
   ```bash
   cd src/TaskBoard.API
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. The API will be available at `https://localhost:7000` and `http://localhost:5000`

#### Option 3: Using Docker (if available)
```bash
docker build -t taskboard-api .
docker run -p 5000:80 -p 7001:443 taskboard-api
```

### Database Migration

The application automatically applies migrations on startup using `MigrateAsync()`. The database will be created with the required tables if it doesn't exist.

### Testing the Setup

1. Open `https://localhost:7000/swagger` in your browser
2. You should see the Swagger UI with all available endpoints
3. Try creating a task using the POST `/api/tasks` endpoint

## API Details

### Base URL
- Development: `https://localhost:7000` or `http://localhost:5000`
- Production: Configure as needed

### Authentication
Currently **not implemented** - all endpoints are publicly accessible.

### Core Endpoints

#### Task Management (`/api/tasks`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tasks` | Get all tasks with filtering and pagination | No |
| GET | `/api/tasks/{id}` | Get specific task with activities | No |
| POST | `/api/tasks` | Create a new task | No |
| PUT | `/api/tasks/{id}` | Update task (full update) | No |
| PATCH | `/api/tasks/{id}/status` | Update task status only | No |
| DELETE | `/api/tasks/{id}` | Soft delete a task | No |

#### Activity Management (`/api/tasks/activities`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tasks/activities` | Get all activities (placeholder) | No |
| GET | `/api/tasks/activities/task/{taskId}` | Get activities for specific task | No |

### Query Parameters

#### For GET `/api/tasks`:
- `status`: Filter by task status (`Todo`, `InProgress`, `Done`)
- `page`: Page number (default: 1, min: 1)
- `pageSize`: Items per page (default: 10, min: 1, max: 100)

### Headers

#### For Concurrency Control:
- `If-Match`: Base64 encoded RowVersion for optimistic concurrency (required for PUT and PATCH operations)

### Request/Response Formats

#### Standard API Response:
```json
{
  "success": true,
  "data": {},
  "message": "Operation completed successfully",
  "errors": []
}
```

#### Create Task Request:
```json
{
  "title": "Sample Task",
  "description": "Task description",
  "priority": "Medium"
}
```

#### Update Task Request:
```json
{
  "title": "Updated Task Title",
  "description": "Updated description",
  "priority": "High"
}
```

#### Update Status Request:
```json
{
  "status": "InProgress"
}
```

### Status Transitions

Valid status transitions enforced by the API:
- ✅ Todo → InProgress
- ✅ InProgress → Done
- ✅ Done → InProgress (reopening)
- ✅ InProgress → Todo (moving back)
- ✅ Done → Todo (reopening)

### Error Handling

| Status Code | Description | Example Scenarios |
|-------------|-------------|-------------------|
| 200 | Success | Successful GET, POST, PUT, PATCH, DELETE |
| 201 | Created | Task successfully created |
| 400 | Bad Request | Validation errors, invalid status transitions |
| 404 | Not Found | Task not found |
| 412 | Precondition Failed | Concurrency conflicts |
| 500 | Internal Server Error | Unexpected server errors |

### Concurrency Handling

The API implements **optimistic concurrency** using RowVersion:
1. GET a task to retrieve the current RowVersion
2. Include the RowVersion in the `If-Match` header when updating
3. Server validates RowVersion before applying changes
4. Returns 412 status if a conflict occurs

## Design Decisions

### Architecture
- **Clean Architecture**: Separated concerns into Domain, Application, Infrastructure, and API layers
- **SOLID Principles**: Dependency injection, interface segregation, single responsibility
- **Repository Pattern**: Abstract data access with testable interfaces

### Technology Stack
- **.NET 7.0**: Latest stable framework with performance improvements
- **Entity Framework Core**: ORM with change tracking and migrations
- **SQL Server**: Robust relational database with ACID compliance
- **AutoMapper**: Object-to-object mapping for DTOs
- **Swagger/OpenAPI**: Interactive API documentation

### Data Design
- **Soft Delete**: Tasks are marked as deleted rather than physically removed
- **RowVersion**: Timestamp-based concurrency control
- **Activity Logging**: Automatic audit trail for all task operations
- **Status Validation**: Business rules enforced at service layer

### API Design
- **RESTful Principles**: Standard HTTP methods and status codes
- **Consistent Response Format**: Standardized API response wrapper
- **Pagination**: Prevents large result sets and improves performance
- **Filtering**: Query parameters for data filtering
- **Global Exception Handling**: Centralized error processing

### Security Considerations
- **Input Validation**: Model validation and custom business rules
- **SQL Injection Protection**: Parameterized queries via EF Core
- **CORS Configuration**: Configurable cross-origin resource sharing
- **HTTPS Ready**: SSL/TLS configuration for production

### Performance Optimizations
- **Async/Await**: Non-blocking I/O operations throughout
- **Efficient Queries**: EF Core query optimization and projection
- **Pagination**: Limits data transfer for large datasets
- **Connection Pooling**: Database connection management

## What is Completed / Not Completed

### Completed Features

#### Core Functionality
- [x] **Task CRUD Operations**: Create, Read, Update, Delete tasks
- [x] **Status Management**: Task status transitions with validation
- [x] **Soft Delete**: Tasks are marked as deleted, not physically removed
- [x] **Activity Logging**: Automatic audit trail for all operations
- [x] **Concurrency Control**: Optimistic concurrency with RowVersion

#### API Features
- [x] **RESTful Endpoints**: Full CRUD API following REST principles
- [x] **Pagination**: Server-side pagination for large datasets
- [x] **Filtering**: Filter tasks by status
- [x] **Global Exception Handling**: Centralized error processing
- [x] **Input Validation**: Model validation and business rules
- [x] **Swagger Documentation**: Interactive API documentation
- [x] **AutoMapper Integration**: DTO mapping between layers

#### Data Layer
- [x] **Entity Framework Core**: Database context and migrations
- [x] **Repository Pattern**: Abstract data access layer
- [x] **Clean Architecture**: Proper separation of concerns
- [x] **Database Migrations**: Automatic schema updates

#### Development Tools
- [x] **Sample HTTP Requests**: Comprehensive test requests file
- [x] **Solution Structure**: Well-organized Visual Studio solution
- [x] **Logging**: Structured logging throughout the application
