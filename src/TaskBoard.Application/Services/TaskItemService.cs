using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using TaskBoard.Application.DTOs;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Application.Services;

public interface ITaskItemService
{
    Task<ApiResponse<TaskItemDto>> CreateAsync(CreateTaskItemDto createDto);
    Task<ApiResponse<TaskItemWithActivitiesDto>> GetByIdAsync(int id);
    Task<ApiResponse<PagedResult<TaskItemDto>>> GetFilteredAsync(TaskBoard.Domain.Enums.TaskStatus? status = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<TaskItemDto>> UpdateAsync(int id, UpdateTaskItemDto updateDto, byte[] rowVersion);
    Task<ApiResponse<TaskItemDto>> PatchStatusAsync(int id, PatchTaskStatusDto patchDto, byte[] rowVersion);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public class TaskItemService : ITaskItemService
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly ITaskActivityRepository _activityRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskItemService> _logger;

    public TaskItemService(
        ITaskItemRepository taskRepository,
        ITaskActivityRepository activityRepository,
        IMapper mapper,
        ILogger<TaskItemService> logger)
    {
        _taskRepository = taskRepository;
        _activityRepository = activityRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<TaskItemDto>> CreateAsync(CreateTaskItemDto createDto)
    {
        try
        {
            var taskItem = _mapper.Map<TaskItem>(createDto);
            
            var createdTask = await _taskRepository.AddAsync(taskItem);
            
            // Log activity
            await LogActivityAsync(createdTask.Id, TaskActivityAction.Created);
            
            var result = _mapper.Map<TaskItemDto>(createdTask);
            
            return new ApiResponse<TaskItemDto>
            {
                Success = true,
                Data = result,
                Message = "Task created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Error creating task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<TaskItemWithActivitiesDto>> GetByIdAsync(int id)
    {
        try
        {
            var taskItem = await _taskRepository.GetByIdAsync(id);
            
            if (taskItem == null)
            {
                return new ApiResponse<TaskItemWithActivitiesDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            var result = _mapper.Map<TaskItemWithActivitiesDto>(taskItem);
            
            return new ApiResponse<TaskItemWithActivitiesDto>
            {
                Success = true,
                Data = result,
                Message = "Task retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with id {TaskId}", id);
            return new ApiResponse<TaskItemWithActivitiesDto>
            {
                Success = false,
                Message = "Error retrieving task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<TaskItemDto>>> GetFilteredAsync(TaskBoard.Domain.Enums.TaskStatus? status = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var tasks = await _taskRepository.GetFilteredAsync(status, page, pageSize);
            var totalCount = await _taskRepository.CountAsync(status);
            
            var taskDtos = _mapper.Map<List<TaskItemDto>>(tasks);
            
            var pagedResult = new PagedResult<TaskItemDto>
            {
                Data = taskDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            return new ApiResponse<PagedResult<TaskItemDto>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Tasks retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filtered tasks");
            return new ApiResponse<PagedResult<TaskItemDto>>
            {
                Success = false,
                Message = "Error retrieving tasks",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<TaskItemDto>> UpdateAsync(int id, UpdateTaskItemDto updateDto, byte[] rowVersion)
    {
        try
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            
            if (existingTask == null)
            {
                return new ApiResponse<TaskItemDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            // Update row version for concurrency check
            existingTask.RowVersion = rowVersion;
            
            _mapper.Map(updateDto, existingTask);
            
            var updatedTask = await _taskRepository.UpdateAsync(existingTask);
            
            // Log activity
            await LogActivityAsync(updatedTask.Id, TaskActivityAction.Updated);
            
            var result = _mapper.Map<TaskItemDto>(updatedTask);
            
            return new ApiResponse<TaskItemDto>
            {
                Success = true,
                Data = result,
                Message = "Task updated successfully"
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating task with id {TaskId}", id);
            return new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Task was modified by another user. Please refresh and try again.",
                Errors = new List<string> { "Concurrency conflict" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with id {TaskId}", id);
            return new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Error updating task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<TaskItemDto>> PatchStatusAsync(int id, PatchTaskStatusDto patchDto, byte[] rowVersion)
    {
        try
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            
            if (existingTask == null)
            {
                return new ApiResponse<TaskItemDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            // Validate status transition
            if (!IsValidStatusTransition(existingTask.Status, patchDto.Status))
            {
                return new ApiResponse<TaskItemDto>
                {
                    Success = false,
                    Message = $"Invalid status transition from {existingTask.Status} to {patchDto.Status}",
                    Errors = new List<string> { "Status transition validation failed" }
                };
            }

            // Update row version for concurrency check
            existingTask.RowVersion = rowVersion;
            
            var oldStatus = existingTask.Status;
            existingTask.Status = patchDto.Status;
            
            var updatedTask = await _taskRepository.UpdateAsync(existingTask);
            
            // Log status change activity
            await LogActivityAsync(updatedTask.Id, TaskActivityAction.StatusChanged);
            
            var result = _mapper.Map<TaskItemDto>(updatedTask);
            
            return new ApiResponse<TaskItemDto>
            {
                Success = true,
                Data = result,
                Message = $"Task status changed from {oldStatus} to {patchDto.Status} successfully"
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating task status with id {TaskId}", id);
            return new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Task was modified by another user. Please refresh and try again.",
                Errors = new List<string> { "Concurrency conflict" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status with id {TaskId}", id);
            return new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Error updating task status",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var existingTask = await _taskRepository.GetByIdAsync(id);
            
            if (existingTask == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            await _taskRepository.DeleteAsync(existingTask);
            
            // Log activity
            await LogActivityAsync(existingTask.Id, TaskActivityAction.Deleted);
            
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Task deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with id {TaskId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private async Task LogActivityAsync(int taskId, TaskActivityAction action)
    {
        var activity = new TaskActivity
        {
            TaskItemId = taskId,
            Action = action,
            Timestamp = DateTime.UtcNow
        };

        await _activityRepository.AddAsync(activity);
    }

    private bool IsValidStatusTransition(TaskBoard.Domain.Enums.TaskStatus fromStatus, TaskBoard.Domain.Enums.TaskStatus toStatus)
    {
        return (fromStatus, toStatus) switch
        {
            (TaskBoard.Domain.Enums.TaskStatus.Todo, TaskBoard.Domain.Enums.TaskStatus.InProgress) => true,
            (TaskBoard.Domain.Enums.TaskStatus.InProgress, TaskBoard.Domain.Enums.TaskStatus.Done) => true,
            (TaskBoard.Domain.Enums.TaskStatus.Done, TaskBoard.Domain.Enums.TaskStatus.InProgress) => true, // Allow reopening
            (TaskBoard.Domain.Enums.TaskStatus.InProgress, TaskBoard.Domain.Enums.TaskStatus.Todo) => true, // Allow moving back
            (TaskBoard.Domain.Enums.TaskStatus.Done, TaskBoard.Domain.Enums.TaskStatus.Todo) => true, // Allow reopening
            _ when fromStatus == toStatus => true, // Allow same status
            _ => false
        };
    }
}
