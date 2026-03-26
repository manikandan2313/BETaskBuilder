using TaskBoard.Domain.Enums;
using TaskStatus = TaskBoard.Domain.Enums.TaskStatus;

namespace TaskBoard.Application.DTOs;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskBoard.Domain.Enums.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskBoard.Domain.Enums.TaskStatus Status { get; set; } = TaskBoard.Domain.Enums.TaskStatus.Todo;
    public TaskPriority Priority { get; set; }
}

public class UpdateTaskItemDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority? Priority { get; set; }
}

public class PatchTaskStatusDto
{
    public TaskBoard.Domain.Enums.TaskStatus Status { get; set; }
}

public class TaskActivityDto
{
    public int Id { get; set; }
    public TaskActivityAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TaskItemWithActivitiesDto : TaskItemDto
{
    public List<TaskActivityDto> Activities { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
