namespace shadcn_taks_api.Persistence.Requests;

public class CreateTagRequest
{
    public int? Id { get; set; } // If Id == null or not set that mean creating new tag, otherwise do nothing.
    public required string Name { get; set; }
}