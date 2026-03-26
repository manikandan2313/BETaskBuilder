using System;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Domain.Entities;

public class TaskActivity
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskActivityAction Action { get; set; }
    public DateTime Timestamp { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
}
