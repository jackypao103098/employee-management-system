using EmployeeManagement.Api.Contracts.Auth;

namespace EmployeeManagement.Api.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
