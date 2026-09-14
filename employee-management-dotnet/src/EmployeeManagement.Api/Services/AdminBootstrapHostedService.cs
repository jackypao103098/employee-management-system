using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services;

public sealed class AdminBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<AdminBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = configuration["AdminBootstrap:Email"];
        var password = configuration["AdminBootstrap:Password"];

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "AdminBootstrap__Email and AdminBootstrap__Password must be provided together.");
        }

        if (password.Length < 12)
        {
            throw new InvalidOperationException(
                "AdminBootstrap__Password must contain at least 12 characters.");
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHashingService = scope.ServiceProvider
            .GetRequiredService<IPasswordHashingService>();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var employee = await dbContext.Employees.SingleOrDefaultAsync(
            employee => employee.Email == normalizedEmail,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Admin bootstrap account must match an existing employee.");

        if (employee.Role == EmployeeRole.Admin)
        {
            logger.LogInformation(
                "Admin bootstrap skipped because employee {EmployeeId} is already an admin.",
                employee.Id);
            return;
        }

        employee.Role = EmployeeRole.Admin;
        employee.PasswordHash = passwordHashingService.Hash(password);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Admin bootstrap completed for employee {EmployeeId}. " +
            "Remove AdminBootstrap environment variables before the next start.",
            employee.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
