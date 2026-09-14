using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Contracts.Employees;

public sealed class EmployeeQueryRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? Department { get; init; }

    [EnumDataType(typeof(HireDateSortDirection))]
    public HireDateSortDirection SortDirection { get; init; } = HireDateSortDirection.Desc;

    [Range(1, 1_000_000)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public enum HireDateSortDirection
{
    Asc,
    Desc
}
