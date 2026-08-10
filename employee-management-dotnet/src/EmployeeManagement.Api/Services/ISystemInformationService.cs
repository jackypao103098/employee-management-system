using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Services;

/// <summary>
/// Provides information about the running API instance.
/// </summary>
public interface ISystemInformationService
{
    /// <summary>
    /// Gets the current system information.
    /// </summary>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The current system information.</returns>
    Task<SystemInformationResponse> GetSystemInformationAsync(
        CancellationToken cancellationToken = default);
}
