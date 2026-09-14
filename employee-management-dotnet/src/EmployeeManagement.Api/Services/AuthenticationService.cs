using System.Security.Cryptography;
using System.Text;
using EmployeeManagement.Api.Contracts.Auth;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Errors;
using EmployeeManagement.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services;

public sealed class AuthenticationService(
    AppDbContext dbContext,
    IPasswordHashingService passwordHashingService,
    IJwtTokenService jwtTokenService,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    private const string DummyPasswordHash =
        "pbkdf2-sha512$210000$ZHVtbXktbG9naW4tc2FsdA==$" +
        "okaUZyjpp8HDpMr0rIS4nRvvhND/bjtVbKWWM/zfJM8=";

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var employee = await dbContext.Employees.SingleOrDefaultAsync(
            employee => employee.Email == normalizedEmail,
            cancellationToken);
        var verification = passwordHashingService.Verify(
            request.Password,
            employee?.PasswordHash ?? DummyPasswordHash);

        if (employee is null || verification == PasswordVerificationResult.Failed)
        {
            logger.LogWarning(
                "Login failed for account fingerprint {AccountFingerprint}.",
                CreateAccountFingerprint(normalizedEmail));
            throw new InvalidCredentialsException();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            employee.PasswordHash = passwordHashingService.Hash(request.Password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var response = jwtTokenService.Create(employee);
        logger.LogInformation(
            "Login succeeded for employee {EmployeeId} with role {Role}.",
            employee.Id,
            employee.Role);

        return response;
    }

    private static string CreateAccountFingerprint(string normalizedEmail)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedEmail));
        return Convert.ToHexString(hash.AsSpan(0, 6));
    }
}
