using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using EmployeeManagement.Api.Contracts.Auth;
using EmployeeManagement.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace EmployeeManagement.Api.Tests;

[Trait("Category", "PostgreSQL")]
public sealed class PostgreSqlEmployeesApiTests(
    PostgreSqlEmployeeApiFactory factory)
    : IClassFixture<PostgreSqlEmployeeApiFactory>
{
    [Fact]
    public async Task CrudWorkflow_UsesMigratedPostgreSqlDatabase()
    {
        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                email = "ada.lovelace@example.com",
                password = factory.AdminPassword
            },
            CancellationToken.None);
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(
            CancellationToken.None);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var email = $"postgres-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "PostgreSQL Integration",
                email,
                age = 30,
                gender = "FEMALE",
                password = "EmployeePass123!",
                department = "Platform",
                hireDate = "2026-09-14"
            },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(
            CancellationToken.None);
        var employeeId = created.GetProperty("id").GetInt32();

        var getResponse = await client.GetAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        var persisted = await getResponse.Content.ReadFromJsonAsync<JsonElement>(
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(email, persisted.GetProperty("email").GetString());
        Assert.Equal("Platform", persisted.GetProperty("department").GetString());

        var duplicateResponse = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Duplicate PostgreSQL Integration",
                email,
                age = 31,
                gender = "MALE",
                password = "EmployeePass123!"
            },
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await client.GetAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }
}

public sealed class PostgreSqlEmployeeApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string JwtIssuer = "EmployeeManagement.PostgreSqlTests";
    private const string JwtAudience = "EmployeeManagement.PostgreSqlTests.Client";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("employee_management_tests")
        .WithUsername("employee_tests")
        .WithPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)))
        .Build();

    private readonly string _jwtSecret = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(64));

    public string AdminPassword { get; } =
        $"Admin9-{Convert.ToHexString(RandomNumberGenerator.GetBytes(12))}a!";

    public async Task InitializeAsync()
    {
        await _database.StartAsync(CancellationToken.None);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_database.GetConnectionString())
            .Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync(CancellationToken.None);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:EmployeeDatabase",
            _database.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:Secret", _jwtSecret);
        builder.UseSetting("Jwt:ExpirationMinutes", "15");
        builder.UseSetting("AdminBootstrap:Email", "ada.lovelace@example.com");
        builder.UseSetting("AdminBootstrap:Password", AdminPassword);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_database.GetConnectionString()));
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await _database.DisposeAsync();
    }
}
