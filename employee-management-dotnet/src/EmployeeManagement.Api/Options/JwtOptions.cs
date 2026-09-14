using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(1)]
    public string Issuer { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    [Range(1, 1_440)]
    public int ExpirationMinutes { get; init; } = 60;
}
