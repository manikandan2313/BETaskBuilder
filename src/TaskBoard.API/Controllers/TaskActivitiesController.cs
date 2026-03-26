using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskBoard.Application.DTOs;
using TaskBoard.Application.Services;
using TaskBoard.Domain.Enums;

namespace TaskBoard.API.Controllers;

[ApiController]
[Route("api/tasks/activities")]
public class TaskActivitiesController : ControllerBase
{
    private readonly ITaskItemService _taskService;
    private readonly ILogger<TaskActivitiesController> _logger;

    public TaskActivitiesController(ITaskItemService taskService, ILogger<TaskActivitiesController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskActivityDto>>>> GetActivities()
    {
        try
        {
            // For now, return empty list since we don't have a dedicated service for activities
            var result = new ApiResponse<List<TaskActivityDto>>
            {
                Success = true,
                Data = new List<TaskActivityDto>(),
                Message = "Activities retrieved successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities");
            return BadRequest(new ApiResponse<List<TaskActivityDto>>
            {
                Success = false,
                Message = "Error retrieving activities",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<ApiResponse<List<TaskActivityDto>>>> GetActivitiesByTask(int taskId)
    {
        try
        {
            var taskResult = await _taskService.GetByIdAsync(taskId);
            
            if (!taskResult.Success)
            {
                if (taskResult.Message.Contains("not found"))
                {
                    return NotFound(taskResult);
                }
                return BadRequest(taskResult);
            }

            var result = new ApiResponse<List<TaskActivityDto>>
            {
                Success = true,
                Data = taskResult.Data?.Activities?.ToList() ?? new List<TaskActivityDto>(),
                Message = "Task activities retrieved successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for task {TaskId}", taskId);
            return BadRequest(new ApiResponse<List<TaskActivityDto>>
            {
                Success = false,
                Message = "Error retrieving task activities",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
