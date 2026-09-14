using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagement.Api.Contracts.Auth;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Api.Security;

public sealed class JwtTokenService(
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public LoginResponse Create(Employee employee)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_jwtOptions.ExpirationMinutes);
        var role = EmployeeRoleNames.From(employee.Role);
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                employee.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Email, employee.Email),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAt,
            new AuthenticatedEmployeeResponse(employee.Id, employee.Email, role));
    }
}
