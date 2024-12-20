using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence.Dtos;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using shadcn_taks_api.Persistence.Requests;
using Task = shadcn_taks_api.Persistence.Entities.Task;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShadcnTaskDbContext>();
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

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

app.MapGet("/tags", async (ShadcnTaskDbContext dbContext) =>
{
    try
    {
        var tags = await dbContext.Tags.AsNoTracking().Select(i => new TagDto()
        {
            Id = i.Id,
            Name = i.Name,
            Tasks = i.Tasks.Select(t => new TaskPreloadDto()
            {
                Id = t.Id,
                Name = t.Name,
                Title = t.Title,
            }).ToList(),
        }).ToListAsync();

        return Results.Ok(tags);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("GetTags").WithOpenApi();

app.MapPost("/tags", async (CreateTagRequest payload, ShadcnTaskDbContext dbContext) =>
{
    try
    {
        if (payload.Id.HasValue)
        {
            return Results.BadRequest("Id must be null or not set for create tag.");
        }

        var newTag = new Tag()
        {
            Name = payload.Name,
        };

        await dbContext.Tags.AddAsync(newTag);
        await dbContext.SaveChangesAsync();

        // Create response data
        var res = new TagDto()
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

        return Results.Created($"/tags/{res.Id}", res);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("CreateTagDto").WithOpenApi();

#endregion

#region TasksEndpoints

app.MapGet("/tasks", async (ShadcnTaskDbContext dbContext) =>
    {
        try
        {
            var tasks = await dbContext.Tasks.AsNoTracking()
                .Include(i => i.TaskTags)
                .Include(i => i.Tags)
                .Select(i => new TaskDto()
                {
                    Id = i.Id,
                    Name = i.Name,
                    Title = i.Title,
                    Tags = i.Tags.Select(t => new TagPreloadDto()
                    {
                        Id = t.Id,
                        Name = t.Name,
                    }).ToList(),
                })
                .ToListAsync();

            return Results.Ok(tasks);
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
                Tags = newTask.Tags.Select(t => new TagPreloadDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList(),
            };

            return Results.Created($"/tasks/{taskDto.Id}", taskDto);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e);
        }
    }).WithName("AddTask")
    .WithOpenApi();

#endregion


/* APP RUN */
app.Run();