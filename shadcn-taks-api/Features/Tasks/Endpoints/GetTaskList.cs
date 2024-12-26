using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Extensions;
using shadcn_taks_api.Models;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tasks.Dtos;
using shadcn_taks_api.Features.Tasks.Models;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class GetTaskList
{
    public static void MapGetTaskList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks",
                async ([AsParameters] GetTaskListRequest req, ShadcnTaskDbContext dbContext, IMapper mapper) =>
                {
                    var tasks = await dbContext.Tasks
                        .AsNoTracking()
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Name), i => i.Name.Contains(req.Name!.Trim()))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Title), i => i.Title.Contains(req.Title!.Trim()))
                        .WhereIf(req.TagIds is { Length: > 0 }, i => i.TaskTags.Any(x => req.TagIds!.Contains(x.TagId)))
                        .WhereIf(req.Statuses is { Length: > 0 }, i => req.Statuses!.Contains(i.Status))
                        .WhereIf(req.Priorities is { Length: > 0 }, i => req.Priorities!.Contains(i.Priority))
                        .Sortable(req)
                        .Paginate(req)
                        .Include(t => t.Tags)
                        .ToListAsync();

                    // Response pagination
                    if (req is { PageNumber: > 0, PageSize: > 0 })
                    {
                        var allTasksCount = await dbContext.Tags.CountAsync();
                        var pagination = new PaginationResponse<TaskDto>()
                        {
                            PageNumber = req.PageNumber.Value,
                            PageSize = req.PageSize.Value,
                            List = tasks.Select(mapper.Map<TaskDto>).ToList(),
                            TotalItems = allTasksCount,
                            TotalPages = (int)Math.Ceiling((double)allTasksCount / req.PageSize.Value),
                        };

                        return TypedResults.Ok(pagination);
                    }

                    // Response all
                    var getAll = new PaginationResponse<TaskDto>()
                    {
                        PageNumber = 0,
                        PageSize = 0,
                        List = tasks.Select(mapper.Map<TaskDto>).ToList(),
                        TotalItems = tasks.Count,
                        TotalPages = 0,
                    };

                    return TypedResults.Ok(getAll);
                })
            .WithName("GetTaskList")
            .WithOpenApi();
    }
}