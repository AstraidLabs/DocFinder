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

    [Fact]
    public async Task LoadReturnsDefaultsWhenFileMissing()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var service = new SettingsService(temp);
        var loaded = await service.LoadAsync();

        Assert.Equal("Light", loaded.Theme);
        Assert.False(loaded.EnableOcr);
        Assert.Empty(loaded.WatchedRoots);
        Assert.Equal(20, loaded.PageSize);
    }

    [Fact]
    public async Task LoadMergesPartialSettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "settings.json");
        await File.WriteAllTextAsync(file, "{\n  \"EnableOcr\": true\n}");
        var service = new SettingsService(file);
        var loaded = await service.LoadAsync();

        Assert.True(loaded.EnableOcr);
        // Theme was missing in file so default should be applied
        Assert.Equal("Light", loaded.Theme);
        Assert.Equal(20, loaded.PageSize);
    }

    [Fact]
    public async Task SaveAndReloadPersistsChanges()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var service = new SettingsService(temp);
        var settings = new AppSettings { Theme = "Dark", EnableOcr = true, WatchedRoots = { "/a" }, PageSize = 42 };
        await service.SaveAsync(settings);

        var service2 = new SettingsService(temp);
        var loaded = await service2.LoadAsync();

        Assert.Equal("Dark", loaded.Theme);
        Assert.True(loaded.EnableOcr);
        Assert.Contains("/a", loaded.WatchedRoots);
        Assert.Equal(42, loaded.PageSize);
    }

    [Fact]
    public async Task SaveResetToDefaults()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var service = new SettingsService(temp);
        await service.SaveAsync(new AppSettings { Theme = "Dark", PageSize = 30 });
        await service.SaveAsync(new AppSettings()); // reset

        var service2 = new SettingsService(temp);
        var loaded = await service2.LoadAsync();
        Assert.Equal("Light", loaded.Theme);
        Assert.Equal(20, loaded.PageSize);
    }
}
