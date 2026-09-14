using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Contracts.Employees;

public sealed record EmployeeResponse(
    int Id,
    string Name,
    string Email,
    int Age,
    Gender Gender,
    string Department,
    DateOnly HireDate);
