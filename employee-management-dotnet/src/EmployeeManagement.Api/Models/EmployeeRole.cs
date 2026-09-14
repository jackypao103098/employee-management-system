namespace EmployeeManagement.Api.Models;

public enum EmployeeRole
{
    Employee,
    Admin
}

public static class EmployeeRoleNames
{
    public const string Employee = "EMPLOYEE";
    public const string Admin = "ADMIN";

    public static string From(EmployeeRole role) => role switch
    {
        EmployeeRole.Admin => Admin,
        _ => Employee
    };
}
