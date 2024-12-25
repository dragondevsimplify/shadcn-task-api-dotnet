using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Models;
using shadcn_taks_api.Persistence.Contexts;
using shadcn_taks_api.Features.Tasks.Dtos;
using shadcn_taks_api.Persistence.Requests;
using Task = shadcn_taks_api.Persistence.Entities.Task;

namespace shadcn_taks_api.Features.Tasks.Endpoints;

public static class GetTaskList
{
    public static void MapGetTaskList(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", async ([AsParameters] GetTaskListRequest req, ShadcnTaskDbContext dbContext, IMapper mapper) =>
    {
        var tasksQuery = dbContext.Tasks.AsNoTracking();

        // Sorting
        if (!string.IsNullOrEmpty(req.SortBy) && !string.IsNullOrEmpty(req.SortOrder.ToString()))
        {
            var sortBy = req.SortBy.ToLower();
            var sortOrder = req.SortOrder.ToString()!.ToUpper();

            Expression<Func<Task, object>> keySelector = sortBy switch
            {
                "name" => t => t.Name,
                "title" => t => t.Title,
                _ => t => t.Id
            };

            tasksQuery = sortOrder == "ASC"
                ? tasksQuery.OrderBy(keySelector)
                : tasksQuery.OrderByDescending(keySelector);
        }

        // Filtering
        if (!string.IsNullOrEmpty(req.Name?.Trim()))
        {
            tasksQuery = tasksQuery.Where(t => t.Name.Contains(req.Name.Trim()));
        }

        if (!string.IsNullOrEmpty(req.Title?.Trim()))
        {
            tasksQuery = tasksQuery.Where(t => t.Title.Contains(req.Title.Trim()));
        }

        if (req.TagIds is { Length: > 0 })
        {
            tasksQuery = tasksQuery.Where(t => t.TaskTags.Any(tt => req.TagIds.Contains(tt.TagId)));
        }

        if (req.Statuses is { Length: > 0 })
        {
            tasksQuery = tasksQuery.Where(t => req.Statuses.Contains(t.Status));
        }

        if (req.Priorities is { Length: > 0 })
        {
            tasksQuery = tasksQuery.Where(t => req.Priorities.Contains(t.Priority));
        }

        // Get all tasks
        var tasks = await tasksQuery
            .Include(i => i.Tags)
            .ToListAsync();

        // Pagination
        if (req is { Page: not null, PageSize: not null })
        {
            var page = req.Page.Value;
            var pageSize = req.PageSize.Value;

            if (page > 0 && pageSize > 0)
            {
                var offset = (page - 1) * pageSize;
                var pagedTasks = await tasksQuery
                    .Skip(offset)
                    .Take(pageSize)
                    .Include(i => i.Tags)
                    .ToListAsync();

                var pagination = new PaginationResponse<TaskDto>()
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    List = pagedTasks.Select(mapper.Map<TaskDto>).ToList(),
                    TotalItems = tasks.Count,
                    TotalPages = (int)Math.Ceiling((double)tasks.Count / pageSize),
                };

                return TypedResults.Ok(pagination);
            }
        }

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