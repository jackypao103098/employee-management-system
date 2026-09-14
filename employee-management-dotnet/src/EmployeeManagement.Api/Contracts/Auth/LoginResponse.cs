namespace EmployeeManagement.Api.Contracts.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedEmployeeResponse Employee);

public sealed record AuthenticatedEmployeeResponse(
    int Id,
    string Email,
    string Role);
