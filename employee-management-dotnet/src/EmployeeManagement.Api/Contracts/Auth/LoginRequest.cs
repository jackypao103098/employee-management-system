using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Contracts.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);
