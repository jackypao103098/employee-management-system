using EmployeeManagement.Api.Security;

namespace EmployeeManagement.Api.Tests.Security;

public sealed class Pbkdf2PasswordHashingServiceTests
{
    private readonly Pbkdf2PasswordHashingService _service = new();

    [Fact]
    public void HashAndVerify_WithCorrectPassword_Succeeds()
    {
        var hash = _service.Hash("correct-horse-battery-staple");

        Assert.StartsWith("pbkdf2-sha512$210000$", hash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _service.Verify("correct-horse-battery-staple", hash));
        Assert.Equal(
            PasswordVerificationResult.Failed,
            _service.Verify("wrong-password", hash));
    }

    [Fact]
    public void Verify_WithLegacyValidHash_RequestsRehash()
    {
        const string legacyHash =
            "pbkdf2-sha256$100000$dGVzdC1zYWx0LTEyMzQ1Ng==$" +
            "1eb7TW62Y2YH1LDOH+7rTcHI80/14qWtDPRZUteOWy4=";

        Assert.Equal(
            PasswordVerificationResult.SuccessRehashNeeded,
            _service.Verify("password123", legacyHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-password-hash")]
    [InlineData("pbkdf2-sha512$999999999$bad$bad")]
    public void Verify_WithMalformedHash_FailsWithoutThrowing(string hash)
    {
        Assert.Equal(
            PasswordVerificationResult.Failed,
            _service.Verify("password123", hash));
    }
}
