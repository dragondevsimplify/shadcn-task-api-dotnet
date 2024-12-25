namespace shadcn_taks_api.Features.Tags.Models;

public class UpdateTagRequest
{
    public int Id { get; set; }
    public required string Name { get; set; }
}