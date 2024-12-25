using System.Text.Json.Serialization;
using shadcn_taks_api.Features.Tags.Endpoints;
using shadcn_taks_api.Features.Tasks.Endpoints;
using shadcn_taks_api.Persistence.Contexts;

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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


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

app.MapGetTagList();
app.MapGetTagById();
app.MapCreateTag();
app.MapUpdateTag();
app.MapDeleteTag();

#endregion

#region TasksEndpoints

app.MapGetTaskList();
app.MapGetTaskStatusList();
app.MapGetTaskPriorityList();
app.MapGetTaskById();
app.MapCreateTask();
app.MapUpdateTask();
app.MapDeleteTask();

#endregion


/* APP RUN */
app.Run();