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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddOptions<RequestTimingOptions>()
    .Bind(builder.Configuration.GetSection(RequestTimingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Datele din memorie trebuie să trăiască între cereri, de aceea repository-urile sunt Singleton
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Demonstrarea duratei de viață a serviciilor
builder.Services.AddTransient<TransientOperation>();
builder.Services.AddScoped<ScopedOperation>();
builder.Services.AddSingleton<SingletonOperation>();
builder.Services.AddTransient<OperationReporter>();

var app = builder.Build();

// Măsurarea în afara handler-ului de erori, ca în log să ajungă codul final al răspunsului
app.UseExceptionHandler();
app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

app.Run();