namespace EmployeeManagement.Api.Errors;

public sealed class DuplicateEmployeeEmailException(string email)
    : Exception($"An employee with email '{email}' already exists.");
