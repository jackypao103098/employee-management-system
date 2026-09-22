using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EmployeeManagement.Api.Contracts.Auth;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Api.Tests;

public sealed class EmployeesApiTests : IClassFixture<EmployeeApiFactory>
{
    private readonly HttpClient _client;
    private readonly EmployeeApiFactory _factory;

    public EmployeesApiTests(EmployeeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CrudWorkflow_ReturnsExpectedStatusCodesAndPersistsChanges()
    {
        await AuthenticateAsAdminAsync();
        var email = $"employee-{Guid.NewGuid():N}@example.com";
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Katherine Johnson",
                email,
                age = 35,
                gender = "FEMALE",
                password = "EmployeePass123!"
            },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var createdJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(
            CancellationToken.None);
        var employeeId = createdJson.GetProperty("id").GetInt32();
        var getResponse = await _client.GetAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/v1/employees/{employeeId}",
            new { name = "Katherine G. Johnson", email, age = 36 },
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidEmail_ReturnsBadRequest()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Invalid Email",
                email = "not-an-email",
                age = 30,
                gender = "MALE",
                password = "EmployeePass123!"
            },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithWeakNumericPassword_ReturnsBadRequest()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Weak Password",
                email = $"weak-{Guid.NewGuid():N}@example.com",
                age = 30,
                gender = "MALE",
                password = "123456789123"
            },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            CreateValidEmployeeRequest($"anonymous-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenAuthenticatedEmployee_ReturnsForbidden()
    {
        await AuthenticateAsEmployeeAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            CreateValidEmployeeRequest($"forbidden-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_ReturnsConflict()
    {
        await AuthenticateAsAdminAsync();
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            name = "First Employee",
            email,
            age = 30,
            gender = "MALE",
            password = "EmployeePass123!"
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            request,
            CancellationToken.None);
        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task CorsPreflight_FromConfiguredReactOrigin_IsAllowed()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/v1/employees");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:5173",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task GetAll_WithQueryParameters_FiltersSortsAndPaginates()
    {
        await AuthenticateAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N");
        await CreateEmployeeAsync(
            $"Search Alice {suffix}",
            $"search-alice-{suffix}@example.com",
            "Engineering",
            "2024-02-01");
        await CreateEmployeeAsync(
            $"Search Bob {suffix}",
            $"search-bob-{suffix}@example.com",
            "Sales",
            "2023-01-01");

        var response = await _client.GetAsync(
            $"/api/v1/employees?name=alice%20{suffix}&department=engineering" +
            "&sortDirection=asc&page=1&pageSize=10",
            CancellationToken.None);
        var employees = await response.Content.ReadFromJsonAsync<JsonElement>(
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var employee = Assert.Single(employees.EnumerateArray());
        Assert.Equal($"Search Alice {suffix}", employee.GetProperty("name").GetString());
        Assert.Equal("Engineering", employee.GetProperty("department").GetString());
        Assert.Equal("2024-02-01", employee.GetProperty("hireDate").GetString());
    }

    [Fact]
    public async Task GetAll_WithPageSizeGreaterThanMaximum_ReturnsBadRequest()
    {
        await AuthenticateAsAdminAsync();
        var response = await _client.GetAsync(
            "/api/v1/employees?pageSize=101",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsVerifiableJwtWithExpectedClaims()
    {
        var response = await LoginAsync("ada.lovelace@example.com", EmployeeApiFactory.AdminPassword);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(login);
        Assert.Equal("Bearer", login.TokenType);

        var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = tokenHandler.ValidateToken(
            login.AccessToken,
            _factory.CreateTokenValidationParameters(),
            out var validatedToken);

        Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal("-1", principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal(
            "ada.lovelace@example.com",
            principal.FindFirstValue(JwtRegisteredClaimNames.Email));
        Assert.Equal("ADMIN", principal.FindFirstValue("role"));
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorizedWithoutEchoingPassword()
    {
        const string password = "incorrect-password";
        var response = await LoginAsync("ada.lovelace@example.com", password);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain(password, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync(
            "/api/v1/employees",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenEmployeeTargetsAnotherEmployee_ReturnsForbidden()
    {
        await AuthenticateAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var first = await CreateEmployeeAndGetIdAsync(
            $"owner-{suffix}@example.com",
            "EmployeePass123!");
        var second = await CreateEmployeeAndGetIdAsync(
            $"other-{suffix}@example.com",
            "EmployeePass123!");
        await AuthenticateAsync($"owner-{suffix}@example.com", "EmployeePass123!");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/employees/{second}",
            new
            {
                name = "Unauthorized update",
                email = $"other-{suffix}@example.com",
                age = 30
            },
            CancellationToken.None);

        Assert.NotEqual(first, second);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenEmployeeTargetsSelf_ReturnsOk()
    {
        await AuthenticateAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"self-{suffix}@example.com";
        var employeeId = await CreateEmployeeAndGetIdAsync(email, "EmployeePass123!");
        await AuthenticateAsync(email, "EmployeePass123!");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/employees/{employeeId}",
            new { name = "Self Updated", email, age = 31 },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenEmployeeIsNotAdmin_ReturnsForbidden()
    {
        await AuthenticateAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"employee-{suffix}@example.com";
        var employeeId = await CreateEmployeeAndGetIdAsync(email, "EmployeePass123!");
        await AuthenticateAsync(email, "EmployeePass123!");

        var response = await _client.DeleteAsync(
            $"/api/v1/employees/{employeeId}",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAdminRoleInBody_StillCreatesEmployeeRole()
    {
        await AuthenticateAsAdminAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"role-spoof-{suffix}@example.com";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Role Spoof",
                email,
                age = 30,
                gender = "MALE",
                password = "EmployeePass123!",
                role = "ADMIN"
            },
            CancellationToken.None);
        response.EnsureSuccessStatusCode();

        var loginResponse = await LoginAsync(email, "EmployeePass123!");
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(
            CancellationToken.None);

        Assert.Equal("EMPLOYEE", login!.Employee.Role);
    }

    [Theory]
    [InlineData("tampered")]
    [InlineData("expired")]
    [InlineData("wrong-audience")]
    public async Task GetAll_WithInvalidToken_ReturnsUnauthorized(string tokenKind)
    {
        var token = tokenKind switch
        {
            "expired" => _factory.CreateTestToken(
                expiresAt: DateTime.UtcNow.AddMinutes(-5)),
            "wrong-audience" => _factory.CreateTestToken(audience: "wrong-audience"),
            _ => _factory.CreateTestToken(signingSecret: "different-signing-secret-that-is-long-enough")
        };
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync(
            "/api/v1/employees",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task CreateEmployeeAsync(
        string name,
        string email,
        string department,
        string hireDate)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name,
                email,
                age = 30,
                gender = "FEMALE",
                password = "EmployeePass123!",
                department,
                hireDate
            },
            CancellationToken.None);

        response.EnsureSuccessStatusCode();
    }

    private async Task<int> CreateEmployeeAndGetIdAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employees",
            new
            {
                name = "Authorization Test",
                email,
                age = 30,
                gender = "FEMALE",
                password
            },
            CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var employee = await response.Content.ReadFromJsonAsync<JsonElement>(
            CancellationToken.None);
        return employee.GetProperty("id").GetInt32();
    }

    private async Task AuthenticateAsAdminAsync() =>
        await AuthenticateAsync("ada.lovelace@example.com", EmployeeApiFactory.AdminPassword);

    private async Task AuthenticateAsEmployeeAsync() =>
        await AuthenticateAsync("grace.hopper@example.com", EmployeeApiFactory.EmployeePassword);

    private async Task AuthenticateAsync(string email, string password)
    {
        var response = await LoginAsync(email, password);
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(
            CancellationToken.None);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password },
            CancellationToken.None);

    private static object CreateValidEmployeeRequest(string email) => new
    {
        name = "Created By Admin",
        email,
        age = 30,
        gender = "FEMALE",
        password = "EmployeePass123!"
    };
}

public sealed class EmployeeApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "EmployeeManagement.Api.Tests";
    public const string JwtAudience = "EmployeeManagement.Api.Tests.Client";
    public const string AdminPassword = "test-admin-password-only";
    public const string EmployeePassword = "test-employee-password-only";

    public string JwtSecret { get; } = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(64));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting(
            "ConnectionStrings:EmployeeDatabase",
            "Host=localhost;Database=unused-in-integration-tests");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:5173");
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:Secret", JwtSecret);
        builder.UseSetting("Jwt:ExpirationMinutes", "60");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            var databaseName = $"employees-api-tests-{Guid.NewGuid():N}";
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
        var admin = dbContext.Employees.Single(employee => employee.Id == -1);
        admin.Role = EmployeeRole.Admin;
        admin.PasswordHash = scope.ServiceProvider
            .GetRequiredService<IPasswordHashingService>()
            .Hash(AdminPassword);
        var employee = dbContext.Employees.Single(employee => employee.Id == -2);
        employee.PasswordHash = scope.ServiceProvider
            .GetRequiredService<IPasswordHashingService>()
            .Hash(EmployeePassword);
        dbContext.SaveChanges();
        return host;
    }

    public TokenValidationParameters CreateTokenValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = JwtIssuer,
        ValidateAudience = true,
        ValidAudience = JwtAudience,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "email",
        RoleClaimType = "role"
    };

    public string CreateTestToken(
        DateTime? expiresAt = null,
        string? audience = null,
        string? signingSecret = null)
    {
        var expiration = expiresAt ?? DateTime.UtcNow.AddMinutes(5);
        var notBefore = expiration <= DateTime.UtcNow
            ? expiration.AddMinutes(-5)
            : DateTime.UtcNow.AddMinutes(-1);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingSecret ?? JwtSecret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: audience ?? JwtAudience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, "123"),
                new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
                new Claim("role", "EMPLOYEE")
            ],
            notBefore: notBefore,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
