using shadcn_taks_api.Persistence.Entities;

namespace shadcn_taks_api.Dtos;

public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }

    public List<TagPreloadDto> Tags { get; set; } = [];
}