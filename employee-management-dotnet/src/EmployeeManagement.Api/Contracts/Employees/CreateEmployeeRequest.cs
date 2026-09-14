using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Contracts.Employees;

public sealed record CreateEmployeeRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Range(16, 100)] int Age,
    [EnumDataType(typeof(Gender))] Gender Gender,
    [Required, StringLength(100, MinimumLength = 8)] string Password,
    [StringLength(100, MinimumLength = 1)] string? Department = null,
    DateOnly? HireDate = null);
