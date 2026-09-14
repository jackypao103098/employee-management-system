using EmployeeManagement.Api.Contracts.Employees;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        EmployeeQueryRequest query,
        CancellationToken cancellationToken);

    Task<EmployeeResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task<EmployeeResponse> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
