using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Entities;

public sealed class Employee
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public required string PasswordHash { get; set; }

    public required string Department { get; set; }

    public DateOnly HireDate { get; set; }

    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;
}
