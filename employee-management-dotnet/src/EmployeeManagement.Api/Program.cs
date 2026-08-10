using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Options;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services
    .AddOptions<ApplicationOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ISystemInformationService, SystemInformationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Employee Management API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = static (context, report) =>
        context.Response.WriteAsJsonAsync(
            new HealthCheckResponse(report.Status.ToString()),
            cancellationToken: context.RequestAborted)
});

app.Run();
