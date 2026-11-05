using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using TaskFlow.Api.Diagnostics;
using TaskFlow.Api.ErrorHandling;
using TaskFlow.Api.Middleware;
using TaskFlow.Api.Options;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Enumerările în JSON se transmit ca șiruri: "High", nu 2
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Handler-ele se înregistrează în ordinea priorității
builder.Services.AddExceptionHandler<ArgumentExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddOptions<RequestTimingOptions>()
    .Bind(builder.Configuration.GetSection(RequestTimingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<DiagnosticsOptions>()
    .Bind(builder.Configuration.GetSection(DiagnosticsOptions.SectionName))
    .ValidateOnStart();

// Datele din memorie trebuie să trăiască între cereri
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
builder.Services.AddSingleton<ITagRepository, InMemoryTagRepository>();

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITagService, TagService>();

// Demonstrarea duratei de viață a serviciilor
builder.Services.AddTransient<TransientOperation>();
builder.Services.AddScoped<ScopedOperation>();
builder.Services.AddSingleton<SingletonOperation>();
builder.Services.AddTransient<OperationReporter>();

var app = builder.Build();

// Ordinea middleware-urilor este importantă
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

app.Run();