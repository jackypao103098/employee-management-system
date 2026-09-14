using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Contracts.Employees;

public sealed record UpdateEmployeeRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Range(16, 100)] int Age,
    [StringLength(100, MinimumLength = 1)] string? Department = null,
    DateOnly? HireDate = null);
