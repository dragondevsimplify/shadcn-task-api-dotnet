using System.Linq.Expressions;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using shadcn_taks_api.Persistence.Responses;
using Task = shadcn_taks_api.Persistence.Entities.Task;
using TaskStatus = shadcn_taks_api.Persistence.Entities.TaskStatus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShadcnTaskDbContext>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAutoMapper(typeof(MappingProfile));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


/* ENDPOINTS */

#region TagsEndpoints

app.MapGet("/tags", async ([AsParameters] GetTagListRequest req, ShadcnTaskDbContext dbContext, IMapper mapper) =>
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

            var pagination = new PaginationDto<TagDto>()
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

    var getAll = new PaginationDto<TagDto>()
    {
        PageNumber = 0,
        PageSize = 0,
        List = tags.Select(mapper.Map<TagDto>).ToList(),
        TotalItems = tags.Count,
        TotalPages = 0,
    };

    return TypedResults.Ok(getAll);
}).WithName("GetTagList").WithOpenApi();

app.MapGet("/tags/{id:int}", async Task<Results<NotFound, Ok<TagDto>>> (int id, ShadcnTaskDbContext dbContext, IMapper mapper) =>
{
    var tag = await dbContext.Tags
        .AsNoTracking()
        .Include(t => t.Tasks)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (tag is null)
    {
        return TypedResults.NotFound();
    }

    var tagDto = mapper.Map<TagDto>(tag);
    return TypedResults.Ok(tagDto);
}).WithName("GetTagById").WithOpenApi();

app.MapPost("/tags",
    async Task<Results<BadRequest<string>, CreatedAtRoute<TagDto>>> (CreateTagRequest payload,
        ShadcnTaskDbContext dbContext, IMapper mapper) =>
    {
        if (payload.Id.HasValue)
        {
            return TypedResults.BadRequest("Id must be null or not set for create tag.");
        }

        var isExist = await dbContext.Tags.AnyAsync(t => t.Name == payload.Name);
        if (isExist)
        {
            return TypedResults.BadRequest("Name is already in use.");
        }

        var newTag = new Tag()
        {
            Name = payload.Name,
        };

        await dbContext.Tags.AddAsync(newTag);
        await dbContext.SaveChangesAsync();

        // Create response data
        var tagDto = mapper.Map<TagDto>(newTag);
        return TypedResults.CreatedAtRoute(tagDto, "GetTagById", new { id = newTag.Id });
    }).WithName("CreateTag").WithOpenApi();

app.MapPut("/tags/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
    int id, Tag tag, ShadcnTaskDbContext dbContext) =>
{
    if (id != tag.Id)
    {
        return TypedResults.BadRequest("Id mismatch.");
    }

    var isExist = await dbContext.Tags.AnyAsync(i => i.Id == id);
    if (!isExist)
    {
        return TypedResults.NotFound();
    }

    isExist = await dbContext.Tags.AnyAsync(t => t.Name == tag.Name && t.Id != tag.Id);
    if (isExist)
    {
        return TypedResults.BadRequest("Name is already in use.");
    }

    dbContext.Update(tag);
    await dbContext.SaveChangesAsync();

    return TypedResults.NoContent();
}).WithName("UpdateTag").WithOpenApi();

app.MapDelete("/tags/{id:int}", async Task<Results<NotFound, NoContent>> (int id, ShadcnTaskDbContext dbContext) =>
{
    var deletedCount = await dbContext.Tags.Where(i => i.Id == id).ExecuteDeleteAsync();

    if (deletedCount == 0)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.NoContent();
}).WithName("DeleteTag").WithOpenApi();

#endregion

#region TasksEndpoints

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

                var pagination = new PaginationDto<TaskDto>()
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

        var getAll = new PaginationDto<TaskDto>()
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

app.MapGet("/tasks/statuses", () =>
{
    var statuses = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>();
    return TypedResults.Ok(statuses);
}).WithName("GetTaskStatusList").WithOpenApi();

app.MapGet("/tasks/priorities", () =>
{
    var priorities = Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>();
    return TypedResults.Ok(priorities);
}).WithName("GetTaskPriorityList").WithOpenApi();

app.MapGet("/tasks/{id:int}", async Task<Results<NotFound, Ok<TaskDto>>> (int id, ShadcnTaskDbContext dbContext, IMapper mapper) =>
{
    var task = await dbContext.Tasks
        .AsNoTracking()
        .Include(t => t.Tags)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (task is null)
    {
        return TypedResults.NotFound();
    }

    var taskDto = mapper.Map<TaskDto>(task);
    return TypedResults.Ok(taskDto);
}).WithName("GetTaskById").WithOpenApi();

app.MapPost("/tasks", async Task<Results<BadRequest<string>, CreatedAtRoute<TaskDto>>> (
        CreateTaskRequest payload, ShadcnTaskDbContext dbContext, IMapper mapper) =>
    {
        // Check if name is existing
        var isExist = await dbContext.Tasks.AnyAsync(t => t.Name == payload.Name);
        if (isExist)
        {
            return TypedResults.BadRequest("Task with the same name already exists");
        }

        // Create tags if not existing in DB
        List<Tag> tags = [];
        foreach (var payloadTag in payload.Tags)
        {
            if (!payloadTag.Id.HasValue)
            {
                var newTag = new Tag()
                {
                    Name = payloadTag.Name,
                };
                await dbContext.Tags.AddAsync(newTag);
                tags.Add(newTag);
            }
            else
            {
                tags.Add(new Tag()
                {
                    Id = payloadTag.Id.Value,
                    Name = payloadTag.Name,
                });
            }
        }

        // Create task
        var newTask = new Task()
        {
            Name = payload.Name,
            Title = payload.Title,
            Status = payload.Status,
            Priority = payload.Priority,
        };
        await dbContext.Tasks.AddAsync(newTask);

        // Save changes
        await dbContext.SaveChangesAsync();

        // Create tasks_tags
        await dbContext.TaskTags.AddRangeAsync(tags.Select(t => new TaskTag()
        {
            TaskId = newTask.Id,
            TagId = t.Id,
        }));
        await dbContext.SaveChangesAsync();

        // Assign tags to response
        newTask.Tags = tags;

        // Create response data
        var taskDto = mapper.Map<TaskDto>(newTask);
        return TypedResults.CreatedAtRoute(taskDto, "GetTaskById", new { id = newTask.Id });
    }).WithName("CreateTask")
    .WithOpenApi();

app.MapPut("/tasks/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (
    int id, UpdateTaskRequest task, ShadcnTaskDbContext dbContext) =>
{
    // Check task Id
    if (id != task.Id)
    {
        return TypedResults.BadRequest("Id mismatch.");
    }

    // Check task is existing
    var isExist = await dbContext.Tasks.AnyAsync(i => i.Id == id);
    if (!isExist)
    {
        return TypedResults.NotFound();
    }

    // Check task name
    isExist = await dbContext.Tasks.AnyAsync(t => t.Name == task.Name && t.Id != id);
    if (isExist)
    {
        return TypedResults.BadRequest("Task with the same name already exists");
    }

    // Delete task tag
    await dbContext.TaskTags.Where(i => i.TaskId == id).ExecuteDeleteAsync();

    // Create tags if not existing in DB
    List<Tag> tags = [];
    foreach (var payloadTag in task.Tags)
    {
        if (!payloadTag.Id.HasValue)
        {
            var isExistTag = await dbContext.Tags.AnyAsync(t => t.Name == payloadTag.Name);
            if (isExistTag)
            {
                return TypedResults.BadRequest($"Tag with the same name already exists: {payloadTag.Name}");
            }

            var newTag = new Tag()
            {
                Name = payloadTag.Name,
            };
            await dbContext.Tags.AddAsync(newTag);
            tags.Add(newTag);
        }
        else
        {
            tags.Add(new Tag()
            {
                Id = payloadTag.Id.Value,
                Name = payloadTag.Name,
            });
        }
    }

    // Update task & save changes
    var taskUpdated = new Task()
    {
        Id = task.Id,
        Name = task.Name,
        Title = task.Title,
        Status = task.Status,
        Priority = task.Priority,
    };
    dbContext.Tasks.Update(taskUpdated);
    await dbContext.SaveChangesAsync();

    // Create tasks_tags
    await dbContext.TaskTags.AddRangeAsync(tags.Select(t => new TaskTag()
    {
        TaskId = id,
        TagId = t.Id,
    }));
    await dbContext.SaveChangesAsync();

    return TypedResults.NoContent();
}).WithName("UpdateTask").WithOpenApi();

app.MapDelete("/tasks/{id:int}", async Task<Results<NotFound, NoContent>> (int id, ShadcnTaskDbContext dbContext) =>
{
    var deletedCount = await dbContext.Tasks.Where(i => i.Id == id).ExecuteDeleteAsync();

    if (deletedCount == 0)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.NoContent();
}).WithName("DeleteTask").WithOpenApi();

#endregion


/* APP RUN */
app.Run();