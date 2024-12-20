namespace shadcn_taks_api.Persistence.Entities;

public class Task
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public List<TaskTag> TaskTags { get; } = [];
    public List<Tag> Tags { get; } = [];

    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
}

public enum TaskStatus
{
    Backlog, Todo, InProgress, Done, Canceled
}

public enum TaskPriority
{
    Low, Medium, High
}