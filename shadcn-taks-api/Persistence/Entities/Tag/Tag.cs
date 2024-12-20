namespace shadcn_taks_api.Persistence.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Task> Tasks { get; } = [];
}