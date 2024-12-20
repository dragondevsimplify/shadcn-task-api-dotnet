namespace shadcn_taks_api.Persistence.Entities;

public class Task
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public List<Tag> Tags { get; set; } = [];
}