using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using shadcn_taks_api.Persistence.Responses;

namespace shadcn_taks_api.Features.Tags.Endpoints;

public static class GetTagList
{
    public static void MapGetTagList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tags",
            async ([AsParameters] GetTagListRequest req, ShadcnTaskDbContext dbContext, IMapper mapper) =>
            {
                var tagsQuery = dbContext.Tags.AsNoTracking();

                // Sorting
                if (!string.IsNullOrEmpty(req.SortBy) && !string.IsNullOrEmpty(req.SortOrder.ToString()))
                {
                    var sortBy = req.SortBy.ToLower();
                    var sortOrder = req.SortOrder.ToString()!.ToUpper();

                    Expression<Func<Tag, object>> keySelector = sortBy switch
                    {
                        "name" => t => t.Name,
                        _ => t => t.Id
                    };

                    tagsQuery = sortOrder == "ASC"
                        ? tagsQuery.OrderBy(keySelector)
                        : tagsQuery.OrderByDescending(keySelector);
                }

                // Filtering
                if (!string.IsNullOrEmpty(req.Name?.Trim()))
                {
                    tagsQuery = tagsQuery.Where(t => t.Name.Contains(req.Name.Trim()));
                }

                // Get all tags
                var tags = await tagsQuery.Include(t => t.Tasks).ToListAsync();

                // Pagination
                if (req is { Page: not null, PageSize: not null })
                {
                    var page = req.Page.Value;
                    var pageSize = req.PageSize.Value;

                    if (page > 0 && pageSize > 0)
                    {
                        var offset = (page - 1) * pageSize;
                        var pagedTags = await tagsQuery.Skip(offset).Take(pageSize).Include(t => t.Tasks).ToListAsync();

                        var pagination = new PaginationResponse<TagDto>()
                        {
                            PageNumber = page,
                            PageSize = pageSize,
                            List = pagedTags.Select(mapper.Map<TagDto>).ToList(),
                            TotalItems = tags.Count,
                            TotalPages = (int)Math.Ceiling((double)tags.Count / pageSize),
                        };

                        return TypedResults.Ok(pagination);
                    }
                }

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