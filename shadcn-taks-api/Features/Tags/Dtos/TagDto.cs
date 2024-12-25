using shadcn_taks_api.Features.Tasks.Dtos;

namespace shadcn_taks_api.Features.Tags.Dtos;

public class TagDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<TaskPreloadDto> Tasks { get; set; } = [];
}