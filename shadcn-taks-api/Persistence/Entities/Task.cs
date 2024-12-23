namespace shadcn_taks_api.Persistence.Entities;

public class Task
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Title { get; set; }

    public List<TaskTag> TaskTags { get; } = [];
    public List<Tag> Tags { get; set; } = [];

    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
}

public enum TaskStatus
{
    Unknown, Backlog, Todo, InProgress, Done, Canceled
}

public enum TaskPriority
{
    Unknown, Low, Medium, High
}