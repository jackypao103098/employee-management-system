using System.Linq.Expressions;
using EmployeeManagement.Api.Contracts.Employees;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Errors;
using EmployeeManagement.Api.Security;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EmployeeManagement.Api.Services;

public sealed class EmployeeService(
    AppDbContext dbContext,
    IPasswordHashingService passwordHashingService,
    TimeProvider timeProvider) : IEmployeeService
{
    private const string DefaultDepartment = "Unassigned";

    private static readonly Expression<Func<Employee, EmployeeResponse>> ProjectToResponse =
        employee => new EmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Age,
            employee.Gender,
            employee.Department,
            employee.HireDate);

    private static readonly Func<Employee, EmployeeResponse> ToResponse =
        ProjectToResponse.Compile();

    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        EmployeeQueryRequest query,
        CancellationToken cancellationToken)
    {
        var employees = dbContext.Employees.AsNoTracking();

        // EF Core translates ToLower() to SQL lower(); the culture-aware and
        // StringComparison overloads suggested by these analyzers cannot be translated.
#pragma warning disable CA1304, CA1311, CA1862

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var normalizedName = query.Name.Trim().ToLowerInvariant();
            employees = employees.Where(employee =>
                employee.Name.ToLower().Contains(normalizedName));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var normalizedDepartment = query.Department.Trim().ToLowerInvariant();
            employees = employees.Where(employee =>
                employee.Department.ToLower() == normalizedDepartment);
        }
#pragma warning restore CA1304, CA1311, CA1862

        var orderedEmployees = query.SortDirection == HireDateSortDirection.Asc
            ? employees.OrderBy(employee => employee.HireDate).ThenBy(employee => employee.Id)
            : employees.OrderByDescending(employee => employee.HireDate).ThenBy(employee => employee.Id);

        return await orderedEmployees
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProjectToResponse)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == id)
            .Select(ProjectToResponse)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new EmployeeNotFoundException(id);

        return employee;
    }

    public async Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        await EnsureEmailIsAvailableAsync(normalizedEmail, null, cancellationToken);

        var employee = new Employee
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            Age = request.Age,
            Gender = request.Gender,
            PasswordHash = passwordHashingService.Hash(request.Password),
            Department = NormalizeDepartment(request.Department),
            HireDate = request.HireDate ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)
        };

        dbContext.Employees.Add(employee);
        await SaveChangesAsync(normalizedEmail, cancellationToken);

        return ToResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .SingleOrDefaultAsync(employee => employee.Id == id, cancellationToken)
            ?? throw new EmployeeNotFoundException(id);

        var normalizedEmail = NormalizeEmail(request.Email);
        await EnsureEmailIsAvailableAsync(normalizedEmail, id, cancellationToken);

        employee.Name = request.Name.Trim();
        employee.Email = normalizedEmail;
        employee.Age = request.Age;
        employee.Department = request.Department is null
            ? employee.Department
            : NormalizeDepartment(request.Department);
        employee.HireDate = request.HireDate ?? employee.HireDate;

        await SaveChangesAsync(normalizedEmail, cancellationToken);
        return ToResponse(employee);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .SingleOrDefaultAsync(employee => employee.Id == id, cancellationToken)
            ?? throw new EmployeeNotFoundException(id);

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureEmailIsAvailableAsync(
        string email,
        int? ignoredEmployeeId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Employees.AnyAsync(
            employee => employee.Email == email && employee.Id != ignoredEmployeeId,
            cancellationToken);

        if (exists)
        {
            throw new DuplicateEmployeeEmailException(email);
        }
    }

    private async Task SaveChangesAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateEmployeeEmailException(email);
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string NormalizeDepartment(string? department) =>
        string.IsNullOrWhiteSpace(department)
            ? DefaultDepartment
            : department.Trim();
}
