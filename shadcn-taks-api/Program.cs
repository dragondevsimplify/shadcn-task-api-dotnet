using System.Linq.Expressions;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using shadcn_taks_api.Features.Tags.Endpoints;
using shadcn_taks_api.Features.Tasks.Endpoints;
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