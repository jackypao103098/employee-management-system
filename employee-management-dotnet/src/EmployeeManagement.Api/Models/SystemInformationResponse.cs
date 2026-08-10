namespace EmployeeManagement.Api.Models;

/// <summary>
/// Describes the running Employee Management API instance.
/// </summary>
/// <param name="ApplicationName">The configured application name.</param>
/// <param name="Environment">The current ASP.NET Core environment.</param>
/// <param name="Version">The configured application version.</param>
/// <param name="ServerTimeUtc">The current server time in UTC.</param>
public sealed record SystemInformationResponse(
    string ApplicationName,
    string Environment,
    string Version,
    DateTimeOffset ServerTimeUtc);
