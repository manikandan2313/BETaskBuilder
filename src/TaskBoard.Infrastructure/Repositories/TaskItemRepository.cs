using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Data;

namespace TaskBoard.Infrastructure.Repositories;

public interface ITaskItemRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<IEnumerable<TaskItem>> GetFilteredAsync(TaskBoard.Domain.Enums.TaskStatus? status = null, int page = 1, int pageSize = 10);
    Task<TaskItem> AddAsync(TaskItem taskItem);
    Task<TaskItem> UpdateAsync(TaskItem taskItem);
    Task DeleteAsync(TaskItem taskItem);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(TaskBoard.Domain.Enums.TaskStatus? status = null);
}

public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _context;

    public TaskItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.TaskItems
            .Include(t => t.Activities)
            .Where(t => t.Id == id && !t.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.TaskItems
            .Include(t => t.Activities)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetFilteredAsync(TaskBoard.Domain.Enums.TaskStatus? status = null, int page = 1, int pageSize = 10)
    {
        var query = _context.TaskItems
            .Include(t => t.Activities)
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status.Equals(status.Value));
        }

        return await query
            .OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem taskItem)
    {
        await _context.TaskItems.AddAsync(taskItem);
        await _context.SaveChangesAsync();
        return taskItem;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem taskItem)
    {
        _context.TaskItems.Update(taskItem);
        await _context.SaveChangesAsync();
        return taskItem;
    }

    public async Task DeleteAsync(TaskItem taskItem)
    {
        taskItem.IsDeleted = true;
        _context.TaskItems.Update(taskItem);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TaskItems.AnyAsync(t => t.Id == id);
    }

    public async Task<int> CountAsync(TaskBoard.Domain.Enums.TaskStatus? status = null)
    {
        var query = _context.TaskItems
            .Where(t => !t.IsDeleted)
            .AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(t => t.Status.Equals(status.Value));
        }

        return await query.CountAsync();
    }
}
