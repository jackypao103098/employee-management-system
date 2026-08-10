namespace EmployeeManagement.Api.Models;

/// <summary>
/// Represents the public health endpoint response.
/// </summary>
/// <param name="Status">The aggregate application health status.</param>
public sealed record HealthCheckResponse(string Status);
