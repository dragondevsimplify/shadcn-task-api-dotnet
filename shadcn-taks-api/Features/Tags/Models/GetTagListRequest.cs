using shadcn_taks_api.Models;

namespace shadcn_taks_api.Features.Tags.Models;

public class GetTagListRequest : PaginationParams
{
    public string? Name { get; set; }
}