using EmployeeManagement.Api.Contracts.Employees;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Errors;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Security;
using EmployeeManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Tests.Services;

public sealed class EmployeeServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesEmailHashesPasswordAndPersistsEmployee()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var response = await service.CreateAsync(
            new CreateEmployeeRequest(" Ada Lovelace ", " ADA@Example.COM ", 36, Gender.Female, "password123"),
            CancellationToken.None);

        var persistedEmployee = await dbContext.Employees.SingleAsync(
            CancellationToken.None);
        Assert.Equal("Ada Lovelace", response.Name);
        Assert.Equal("ada@example.com", response.Email);
        Assert.Equal("ada@example.com", persistedEmployee.Email);
        Assert.NotEqual("password123", persistedEmployee.PasswordHash);
        Assert.StartsWith("pbkdf2-sha512$", persistedEmployee.PasswordHash);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var request = new CreateEmployeeRequest(
            "Grace Hopper",
            "grace@example.com",
            40,
            Gender.Female,
            "password123");

        await service.CreateAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateEmployeeEmailException>(() =>
            service.CreateAsync(
                request with { Email = "GRACE@example.com" },
                CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeDoesNotExist_ThrowsNotFoundException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<EmployeeNotFoundException>(() =>
            service.GetByIdAsync(404, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAndDeleteAsync_PersistChanges()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        var created = await service.CreateAsync(
            new CreateEmployeeRequest("Alan Turing", "alan@example.com", 30, Gender.Male, "password123"),
            CancellationToken.None);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateEmployeeRequest("Alan Mathison Turing", "turing@example.com", 31),
            CancellationToken.None);
        await service.DeleteAsync(created.Id, CancellationToken.None);

        Assert.Equal("Alan Mathison Turing", updated.Name);
        Assert.Equal("turing@example.com", updated.Email);
        Assert.Empty(await service.GetAllAsync(new EmployeeQueryRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_FiltersSortsAndPaginatesWithStableIdTieBreaker()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Employees.AddRange(
            CreateEmployee("Alice Chen", "alice@example.com", "Engineering", new DateOnly(2024, 1, 10)),
            CreateEmployee("Bob Wang", "bob@example.com", "Sales", new DateOnly(2024, 1, 10)),
            CreateEmployee("Alicia Lin", "alicia@example.com", "Engineering", new DateOnly(2023, 5, 1)));
        await dbContext.SaveChangesAsync(CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        var service = CreateService(dbContext);

        var firstPage = await service.GetAllAsync(
            new EmployeeQueryRequest
            {
                Name = "ali",
                Department = "engineering",
                SortDirection = HireDateSortDirection.Asc,
                Page = 1,
                PageSize = 1
            },
            CancellationToken.None);
        var secondPage = await service.GetAllAsync(
            new EmployeeQueryRequest
            {
                Name = "ali",
                Department = "engineering",
                SortDirection = HireDateSortDirection.Asc,
                Page = 2,
                PageSize = 1
            },
            CancellationToken.None);

        Assert.Equal("Alicia Lin", Assert.Single(firstPage).Name);
        Assert.Equal("Alice Chen", Assert.Single(secondPage).Name);
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetAllAsync_WithCancelledToken_PropagatesCancellation()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetAllAsync(new EmployeeQueryRequest(), cancellationSource.Token));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static EmployeeService CreateService(AppDbContext dbContext) =>
        new(dbContext, new Pbkdf2PasswordHashingService());

    private static Employee CreateEmployee(
        string name,
        string email,
        string department,
        DateOnly hireDate) =>
        new()
        {
            Name = name,
            Email = email,
            Age = 30,
            Gender = Gender.Female,
            PasswordHash = "test-password-hash",
            Department = department,
            HireDate = hireDate
        };
}
