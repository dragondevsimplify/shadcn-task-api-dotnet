namespace shadcn_taks_api.Features.Tags.Models;

// Create tag when attach to another API
public class CreateTagAttach
{
    public int? Id { get; set; } // If Id == null or not set that mean creating new tag, otherwise do nothing.
    public required string Name { get; set; }
}