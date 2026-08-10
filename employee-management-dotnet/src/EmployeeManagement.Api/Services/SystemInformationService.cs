using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Options;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Api.Services;

/// <summary>
/// Builds system information from validated configuration and the hosting environment.
/// </summary>
public sealed class SystemInformationService : ISystemInformationService
{
    private readonly ApplicationOptions _applicationOptions;
    private readonly string _environmentName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemInformationService"/> class.
    /// </summary>
    public SystemInformationService(
        IOptions<ApplicationOptions> applicationOptions,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(applicationOptions);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        _applicationOptions = applicationOptions.Value;
        _environmentName = hostEnvironment.EnvironmentName;
    }

    /// <inheritdoc />
    public Task<SystemInformationResponse> GetSystemInformationAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new SystemInformationResponse(
            _applicationOptions.Name,
            _environmentName,
            _applicationOptions.Version,
            DateTimeOffset.UtcNow);

        return Task.FromResult(response);
    }
}
