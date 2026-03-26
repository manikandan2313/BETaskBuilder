using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Data;

namespace TaskBoard.Infrastructure.Repositories;

public interface ITaskActivityRepository
{
    Task<TaskActivity> AddAsync(TaskActivity activity);
    Task<IEnumerable<TaskActivity>> GetByTaskIdAsync(int taskId);
}

public class TaskActivityRepository : ITaskActivityRepository
{
    private readonly AppDbContext _context;

    public TaskActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskActivity> AddAsync(TaskActivity activity)
    {
        await _context.TaskActivities.AddAsync(activity);
        return activity;
    }

    public async Task<IEnumerable<TaskActivity>> GetByTaskIdAsync(int taskId)
    {
        return await _context.TaskActivities
            .Where(a => a.TaskItemId == taskId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
