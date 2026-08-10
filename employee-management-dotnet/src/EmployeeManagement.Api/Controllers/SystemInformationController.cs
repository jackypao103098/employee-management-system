using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

/// <summary>
/// Exposes information about the running API instance.
/// </summary>
[ApiController]
[Route("api/v1/system/info")]
public sealed class SystemInformationController(
    ISystemInformationService systemInformationService) : ControllerBase
{
    /// <summary>
    /// Gets the application identity, environment, version, and current UTC time.
    /// </summary>
    /// <param name="cancellationToken">Signals that the HTTP request was aborted.</param>
    /// <returns>The current system information.</returns>
    [HttpGet]
    [ProducesResponseType<SystemInformationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemInformationResponse>> GetAsync(
        CancellationToken cancellationToken)
    {
        var response = await systemInformationService
            .GetSystemInformationAsync(cancellationToken);

        return Ok(response);
    }
}
