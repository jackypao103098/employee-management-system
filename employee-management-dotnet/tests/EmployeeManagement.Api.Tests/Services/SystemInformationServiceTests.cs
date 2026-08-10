using EmployeeManagement.Api.Options;
using EmployeeManagement.Api.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace EmployeeManagement.Api.Tests.Services;

public sealed class SystemInformationServiceTests
{
    [Fact]
    public async Task GetSystemInformationAsync_ReturnsConfiguredApplicationName()
    {
        var service = CreateService();

        var result = await service.GetSystemInformationAsync();

        Assert.Equal("Employee Management API", result.ApplicationName);
    }

    [Fact]
    public async Task GetSystemInformationAsync_ReturnsNonDefaultUtcTime()
    {
        var service = CreateService();

        var result = await service.GetSystemInformationAsync();

        Assert.NotEqual(default, result.ServerTimeUtc);
        Assert.Equal(TimeSpan.Zero, result.ServerTimeUtc.Offset);
    }

    [Fact]
    public async Task GetSystemInformationAsync_ReturnsNonEmptyEnvironmentName()
    {
        var service = CreateService();

        var result = await service.GetSystemInformationAsync();

        Assert.False(string.IsNullOrWhiteSpace(result.Environment));
    }

    private static SystemInformationService CreateService()
    {
        var options = OptionsFactory.Create(new ApplicationOptions
        {
            Name = "Employee Management API",
            Version = "1.0.0"
        });
        var environment = new TestHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        return new SystemInformationService(options, environment);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = "EmployeeManagement.Api.Tests";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}
