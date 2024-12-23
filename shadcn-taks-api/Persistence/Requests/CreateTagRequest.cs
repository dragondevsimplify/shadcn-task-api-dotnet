namespace shadcn_taks_api.Persistence.Requests;

public class CreateTagRequest
{
    public int? Id { get; set; }
    public required string Name { get; set; }
}