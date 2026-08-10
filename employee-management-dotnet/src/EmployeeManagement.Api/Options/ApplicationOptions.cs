using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Options;

/// <summary>
/// Represents identity settings displayed by the API.
/// </summary>
public sealed class ApplicationOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Application";

    /// <summary>Gets or sets the human-readable application name.</summary>
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the application version.</summary>
    [Required(AllowEmptyStrings = false)]
    public string Version { get; set; } = string.Empty;
}
