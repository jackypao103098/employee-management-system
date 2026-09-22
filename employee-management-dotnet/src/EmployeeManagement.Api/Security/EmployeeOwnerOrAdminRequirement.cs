using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Api.Security;

public sealed class EmployeeOwnerOrAdminRequirement : IAuthorizationRequirement;
