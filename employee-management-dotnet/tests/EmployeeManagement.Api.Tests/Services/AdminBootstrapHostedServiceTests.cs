using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Security;
using EmployeeManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeManagement.Api.Tests.Services;

public sealed class AdminBootstrapHostedServiceTests
{
    [Fact]
    public async Task StartAsync_WithInjectedCredentials_PromotesExistingEmployeeOnce()
    {
        const string bootstrapPassword = "bootstrap-test-password";
        await using var provider = CreateServiceProvider();
        await SeedEmployeeAsync(provider);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminBootstrap:Email"] = "owner@example.com",
                ["AdminBootstrap:Password"] = bootstrapPassword
            })
            .Build();
        var service = new AdminBootstrapHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            configuration,
            NullLogger<AdminBootstrapHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await service.StartAsync(CancellationToken.None);

        await using var scope = provider.CreateAsyncScope();
        var employee = await scope.ServiceProvider
            .GetRequiredService<AppDbContext>()
            .Employees.SingleAsync(CancellationToken.None);
        var passwordHashingService = scope.ServiceProvider
            .GetRequiredService<IPasswordHashingService>();
        Assert.Equal(EmployeeRole.Admin, employee.Role);
        Assert.Equal(
            PasswordVerificationResult.Success,
            passwordHashingService.Verify(bootstrapPassword, employee.PasswordHash));
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddSingleton<IPasswordHashingService, Pbkdf2PasswordHashingService>();
        return services.BuildServiceProvider();
    }

    private static async Task SeedEmployeeAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Employees.Add(new Employee
        {
            Name = "Bootstrap Owner",
            Email = "owner@example.com",
            Age = 30,
            Gender = Gender.Female,
            PasswordHash = "not-a-valid-login-hash",
            Department = "Engineering",
            HireDate = new DateOnly(2026, 9, 14)
        });
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
