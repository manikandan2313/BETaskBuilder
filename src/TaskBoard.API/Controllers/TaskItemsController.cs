using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskBoard.Application.DTOs;
using TaskBoard.Application.Services;
using TaskBoard.Domain.Enums;
using TaskStatus = TaskBoard.Domain.Enums.TaskStatus;

namespace TaskBoard.API.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskItemsController : ControllerBase
{
    private readonly ITaskItemService _taskService;
    private readonly ILogger<TaskItemsController> _logger;

    public TaskItemsController(ITaskItemService taskService, ILogger<TaskItemsController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TaskItemDto>>>> GetTasks(
        [FromQuery] TaskBoard.Domain.Enums.TaskStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _taskService.GetFilteredAsync(status, page, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TaskItemWithActivitiesDto>>> GetTask(int id)
    {
        var result = await _taskService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskItemDto>>> CreateTask([FromBody] CreateTaskItemDto createDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = GetModelErrors(ModelState);
            return BadRequest(new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            });
        }

        var result = await _taskService.CreateAsync(createDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetTask), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TaskItemDto>>> UpdateTask(
        int id, 
        [FromBody] UpdateTaskItemDto updateDto,
        [FromHeader(Name = "If-Match")] string? ifMatch = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = GetModelErrors(ModelState);
            return BadRequest(new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            });
        }

        byte[] rowVersion = Array.Empty<byte>();
        if (!string.IsNullOrEmpty(ifMatch))
        {
            try
            {
                rowVersion = Convert.FromBase64String(ifMatch.Trim('"'));
            }
            catch
            {
                return BadRequest(new ApiResponse<TaskItemDto>
                {
                    Success = false,
                    Message = "Invalid If-Match header value"
                });
            }
        }

        var result = await _taskService.UpdateAsync(id, updateDto, rowVersion);
        
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(412, result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<TaskItemDto>>> PatchTaskStatus(
        int id,
        [FromBody] PatchTaskStatusDto patchDto,
        [FromHeader(Name = "If-Match")] string? ifMatch = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = GetModelErrors(ModelState);
            return BadRequest(new ApiResponse<TaskItemDto>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            });
        }

        byte[] rowVersion = Array.Empty<byte>();
        if (!string.IsNullOrEmpty(ifMatch))
        {
            try
            {
                rowVersion = Convert.FromBase64String(ifMatch.Trim('"'));
            }
            catch
            {
                return BadRequest(new ApiResponse<TaskItemDto>
                {
                    Success = false,
                    Message = "Invalid If-Match header value"
                });
            }
        }

        var result = await _taskService.PatchStatusAsync(id, patchDto, rowVersion);
        
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(412, result);
            }
            if (result.Message.Contains("Invalid status transition"))
            {
                return BadRequest(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(int id)
    {
        var result = await _taskService.DeleteAsync(id);
        
        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    private static List<string> GetModelErrors(ModelStateDictionary modelState)
    {
        var errors = new List<string>();
        
        foreach (var state in modelState.Values)
        {
            foreach (var error in state.Errors)
            {
                errors.Add(error.ErrorMessage);
            }
        }
        
        return errors;
    }
}
