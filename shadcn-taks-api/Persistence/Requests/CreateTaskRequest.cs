namespace shadcn_taks_api.Persistence.Requests;

public class CreateTaskRequest
{
    public string Name { get; set; }
    public string Title { get; set; }

    public List<CreateTagRequest> Tags { get; set; } = [];
}