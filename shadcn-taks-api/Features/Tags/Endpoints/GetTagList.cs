using AutoMapper;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tags.Dtos;
using shadcn_taks_api.Features.Tags.Models;
using shadcn_taks_api.Models;
using shadcn_taks_api.Extensions;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class GetTagList
{
    public static void MapGetTagList(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tags/paging",
            async (GetTagListRequest req, ShadcnTaskDbContext dbContext, IMapper mapper) =>
            {
                var tags = await dbContext.Tags
                    .AsNoTracking()
                    .WhereIf(!string.IsNullOrWhiteSpace(req.Name), i => i.Name.Contains(req.Name!.Trim()))
                    .Sortable(req)
                    .Paginate(req)
                    .Include(t => t.Tasks)
                    .ToListAsync();

                // Response pagination
                if (req is { PageNumber: > 0, PageSize: > 0 })
                {
                    var allTagsCount = await dbContext.Tags.CountAsync();
                    var pagination = new PaginationResponse<TagDto>()
                    {
                        PageNumber = req.PageNumber.Value,
                        PageSize = req.PageSize.Value,
                        List = tags.Select(mapper.Map<TagDto>).ToList(),
                        TotalItems = allTagsCount,
                        TotalPages = (int)Math.Ceiling((double)allTagsCount / req.PageSize.Value),
                    };

                    return TypedResults.Ok(pagination);
                }

                // Response all
                var getAll = new PaginationResponse<TagDto>()
                {
                    PageNumber = 0,
                    PageSize = 0,
                    List = tags.Select(mapper.Map<TagDto>).ToList(),
                    TotalItems = tags.Count,
                    TotalPages = 0,
                };

                return TypedResults.Ok(getAll);
            }).WithName("GetTagList").WithOpenApi();
    }
}