namespace EmployeeManagement.Api.Security;

public interface IPasswordHashingService
{
    string Hash(string password);

    PasswordVerificationResult Verify(string password, string passwordHash);
}

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}
