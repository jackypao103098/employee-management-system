using EmployeeManagement.Api.Contracts.Auth;
using EmployeeManagement.Api.Entities;

namespace EmployeeManagement.Api.Security;

public interface IJwtTokenService
{
    LoginResponse Create(Employee employee);
}
