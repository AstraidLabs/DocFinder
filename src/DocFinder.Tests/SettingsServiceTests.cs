using System;
using System.IO;
using System.Threading.Tasks;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using Xunit;

namespace DocFinder.Tests;

public class SettingsServiceTests
{
    [Fact]
    public async Task SaveAddsSourceRoot()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var service = new SettingsService(temp);
        var settings = new AppSettings { SourceRoot = "/data" };
        await service.SaveAsync(settings);

        Assert.Contains("/data", service.Current.WatchedRoots);
        Assert.True(File.Exists(temp));
    }
}
