using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Api.Security;

public static class AuthorizationPolicies
{
    public const string EmployeeOwnerOrAdmin = "EmployeeOwnerOrAdmin";
}

public sealed class EmployeeOwnerOrAdminRequirement : IAuthorizationRequirement;

public sealed class EmployeeOwnerOrAdminHandler
    : AuthorizationHandler<EmployeeOwnerOrAdminRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmployeeOwnerOrAdminRequirement requirement,
        int employeeId)
    {
        if (context.User.IsInRole(EmployeeRoleNames.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var subject = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (int.TryParse(subject, NumberStyles.None, CultureInfo.InvariantCulture, out var userId) &&
            userId == employeeId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
