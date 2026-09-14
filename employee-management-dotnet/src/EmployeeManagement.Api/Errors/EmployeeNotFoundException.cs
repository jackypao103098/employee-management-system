namespace EmployeeManagement.Api.Errors;

public sealed class EmployeeNotFoundException(int employeeId)
    : Exception($"Employee with id '{employeeId}' was not found.");
