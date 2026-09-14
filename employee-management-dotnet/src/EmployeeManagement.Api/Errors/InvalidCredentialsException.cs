namespace EmployeeManagement.Api.Errors;

public sealed class InvalidCredentialsException()
    : Exception("Email or password is incorrect.");
