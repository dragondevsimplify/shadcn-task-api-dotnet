using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Persistence;
using shadcn_taks_api.Persistence.Entities;
using Task = shadcn_taks_api.Persistence.Entities.Task;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ShadcnTaskDbContext>();
builder.Services.AddControllers()
    .AddNewtonsoftJson(x => x.SerializerSettings.SetDefault())
    .AddJsonOptions(x =>
    {
        // x.
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

app.MapGet("/tags", async (ShadcnTaskDbContext dbContext) =>
{
    try
    {
        var tags = await dbContext.Tags.ToListAsync();
        return Results.Ok(tags);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("GetTags").WithOpenApi();

app.MapPost("/tags", async (CreateTag payload, ShadcnTaskDbContext dbContext) =>
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

        return Results.Created($"/tags/{newTag.Id}", newTag);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}).WithName("CreateTag").WithOpenApi();

#endregion

#region TasksEndpoints

app.MapGet("/tasks", async (ShadcnTaskDbContext dbContext) =>
    {
        try
        {
            var tasks = await dbContext.Tasks.AsNoTracking()
                .Include(i => i.Tags)
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

app.MapPost("/tasks", async (CreateTask payload, ShadcnTaskDbContext dbContext) =>
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

            return Results.Created($"/tasks/{newTask.Id}", newTask);
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