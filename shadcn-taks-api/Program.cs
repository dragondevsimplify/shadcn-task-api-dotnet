using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using shadcn_taks_api.Persistence.Responses;
using Task = shadcn_taks_api.Persistence.Entities.Task;

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

app.MapGet("/tags", async ([AsParameters] GetTagListRequest req, ShadcnTaskDbContext dbContext) =>
{
    var tagsTable = dbContext.Tags.AsNoTracking();

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

        tagsTable = sortOrder == "ASC"
            ? tagsTable.OrderBy(keySelector)
            : tagsTable.OrderByDescending(keySelector);
    }

    // Filtering
    if (!string.IsNullOrEmpty(req.Name?.Trim()))
    {
        tagsTable = tagsTable.Where(t => t.Name.Contains(req.Name.Trim()));
    }

    // Get all tags
    var tags = await tagsTable.ToListAsync();

    // Pagination
    if (req.Page.HasValue && req.PageSize.HasValue)
    {
        var page = req.Page.Value;
        var pageSize = req.PageSize.Value;

        if (page > 0 && pageSize > 0)
        {
            var offset = (page - 1) * pageSize;
            var pagedTasks = await tagsTable.Skip(offset).Take(pageSize).ToListAsync();

            var pagination = new PaginationResponse<TagDto>()
            {
                PageNumber = page,
                PageSize = pageSize,
                List = pagedTasks.Select(i => new TagDto()
                {
                    Id = i.Id,
                    Name = i.Name,
                    Tasks = i.Tasks.Select(t => new TaskPreloadDto()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                    }).ToList(),
                }).ToList(),
                TotalItems = tags.Count,
                TotalPages = (int)Math.Ceiling((double)tags.Count / pageSize),
            };

            return Results.Ok(pagination);
        }
    }

    var getAll = new PaginationResponse<TagDto>()
    {
        PageNumber = 0,
        PageSize = 0,
        List = tags.Select(i => new TagDto()
        {
            Id = i.Id,
            Name = i.Name,
            Tasks = i.Tasks.Select(t => new TaskPreloadDto()
            {
                Id = t.Id,
                Name = t.Name,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
            }).ToList(),
        }).ToList(),
        TotalItems = tags.Count,
        TotalPages = 0,
    };

    return TypedResults.Ok(getAll);
}).WithName("GetTags").WithOpenApi();

app.MapGet("/tags/{id:int}", async Task<Results<NotFound, Ok<TagDto>>> (int id, ShadcnTaskDbContext dbContext) =>
{
    var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

    if (tag is null)
    {
        return TypedResults.NotFound();
    }

    var tagDto = new TagDto()
    {
        Id = tag.Id,
        Name = tag.Name,
        Tasks = tag.Tasks.Select(t => new TaskPreloadDto()
        {
            Id = t.Id,
            Name = t.Name,
            Title = t.Title,
        }).ToList(),
    };

    return TypedResults.Ok(tagDto);
}).WithName("GetTag").WithOpenApi();

app.MapPost("/tags",
    async Task<Results<BadRequest<string>, CreatedAtRoute<TagDto>>> (CreateTagRequest payload,
        ShadcnTaskDbContext dbContext) =>
    {
        if (payload.Id.HasValue)
        {
            return TypedResults.BadRequest("Id must be null or not set for create tag.");
        }

        var newTag = new Tag()
        {
            Name = payload.Name,
        };

        await dbContext.Tags.AddAsync(newTag);
        await dbContext.SaveChangesAsync();

        // Create response data
        var tagDto = new TagDto()
        {
            Id = newTag.Id,
            Name = newTag.Name,
            Tasks = newTag.Tasks.Select(t => new TaskPreloadDto()
            {
                Id = t.Id,
                Name = t.Name,
                Title = t.Title,
            }).ToList(),
        };

        return TypedResults.CreatedAtRoute(tagDto, "GetTag", new { id = newTag.Id });
    }).WithName("CreateTagDto").WithOpenApi();

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

app.MapGet("/tasks", async (ShadcnTaskDbContext dbContext) =>
    {
        try
        {
            var tasks = await dbContext.Tasks.AsNoTracking()
                .Include(i => i.TaskTags)
                .Include(i => i.Tags)
                .ToListAsync();

            var res = tasks.Select(i => new TaskDto()
            {
                Id = i.Id,
                Name = i.Name,
                Title = i.Title,
                Tags = i.Tags.Select(t => new TagPreloadDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
                Status = i.Status,
                Priority = i.Priority,
            });

            return Results.Ok(res);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    })
    .WithName("GetTasks")
    .WithOpenApi();

app.MapPost("/tasks", async (CreateTaskRequest payload, ShadcnTaskDbContext dbContext) =>
    {
        try
        {
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

            // Create response data
            var taskDto = new TaskDto()
            {
                Id = newTask.Id,
                Name = newTask.Name,
                Title = newTask.Title,
                Tags = tags.Select(t => new TagPreloadDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
                Status = newTask.Status,
                Priority = newTask.Priority,
            };

            return Results.Created($"/tasks/{taskDto.Id}", taskDto);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }).WithName("AddTask")
    .WithOpenApi();

#endregion


/* APP RUN */
app.Run();