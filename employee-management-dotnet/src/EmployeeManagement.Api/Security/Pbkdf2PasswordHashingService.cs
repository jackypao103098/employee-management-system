using System.Globalization;
using System.Security.Cryptography;

namespace EmployeeManagement.Api.Security;

public sealed class Pbkdf2PasswordHashingService : IPasswordHashingService
{
    private const string CurrentAlgorithm = "pbkdf2-sha512";
    private const int CurrentIterations = 210_000;
    private const int MaximumAcceptedIterations = 1_000_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            CurrentIterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return string.Join(
            '$',
            CurrentAlgorithm,
            CurrentIterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 ||
                !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) ||
                iterations <= 0 || iterations > MaximumAcceptedIterations)
            {
                return PasswordVerificationResult.Failed;
            }

            var algorithm = parts[0] switch
            {
                "pbkdf2-sha256" => HashAlgorithmName.SHA256,
                CurrentAlgorithm => HashAlgorithmName.SHA512,
                _ => default
            };

            if (algorithm == default)
            {
                return PasswordVerificationResult.Failed;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            if (salt.Length < SaltSize || expectedHash.Length != HashSize)
            {
                return PasswordVerificationResult.Failed;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                algorithm,
                expectedHash.Length);

            if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            {
                return PasswordVerificationResult.Failed;
            }

            return parts[0] == CurrentAlgorithm && iterations >= CurrentIterations
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
        catch (CryptographicException)
        {
            return PasswordVerificationResult.Failed;
        }
    }
}
