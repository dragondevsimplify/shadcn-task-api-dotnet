namespace shadcn_taks_api.Persistence.Entities;

public class CreateTask
{
    public string Name { get; set; }
    public string Title { get; set; }

    public List<CreateTag> Tags { get; set; } = [];
}