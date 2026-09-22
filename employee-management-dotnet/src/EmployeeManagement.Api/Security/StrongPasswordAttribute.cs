using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Security;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class StrongPasswordAttribute()
    : ValidationAttribute(PasswordPolicy.ErrorMessage)
{
    public override bool IsValid(object? value) =>
        value is string password && PasswordPolicy.IsStrong(password);
}
