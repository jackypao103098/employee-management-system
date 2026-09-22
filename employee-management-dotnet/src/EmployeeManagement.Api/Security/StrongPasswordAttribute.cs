using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Security;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class StrongPasswordAttribute()
    : ValidationAttribute(PasswordPolicy.ErrorMessage)
{
    public override bool IsValid(object? value) =>
        value is string password && PasswordPolicy.IsStrong(password);
}

public static class PasswordPolicy
{
    public const int MinimumLength = 12;
    public const string ErrorMessage =
        "Password must contain at least 12 characters, including uppercase, " +
        "lowercase, number, and special character.";

    public static bool IsStrong(string password) =>
        password.Length >= MinimumLength &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit) &&
        password.Any(character => !char.IsLetterOrDigit(character));
}
