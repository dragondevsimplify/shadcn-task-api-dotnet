namespace shadcn_taks_api.Features.Tags.Dtos;

// Preload <=> Use Include in LINQ
public class TagPreloadDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}