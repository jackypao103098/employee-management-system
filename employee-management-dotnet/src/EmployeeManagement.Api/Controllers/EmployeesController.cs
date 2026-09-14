using EmployeeManagement.Api.Contracts.Employees;
using EmployeeManagement.Api.Models;
using EmployeeManagement.Api.Security;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public sealed class EmployeesController(
    IEmployeeService employeeService,
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetAll(
        [FromQuery] EmployeeQueryRequest query,
        CancellationToken cancellationToken)
    {
        var employees = await employeeService.GetAllAsync(query, cancellationToken);
        return Ok(employees);
    }

    [HttpGet("{id:int}", Name = nameof(GetById))]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        return Ok(employee);
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            employee);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(
            User,
            id,
            AuthorizationPolicies.EmployeeOwnerOrAdmin);
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var employee = await employeeService.UpdateAsync(id, request, cancellationToken);
        return Ok(employee);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = EmployeeRoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
