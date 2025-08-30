using System;
using System.IO;
using System.Threading.Tasks;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using Xunit;

namespace DocFinder.Tests;

public class SettingsServiceTests
{
    private sealed class FakeHotkeyService : IHotkeyService
    {
        public bool Registered { get; private set; }
        public Hotkey? LastHotkey { get; private set; }
        public Task RegisterAsync(Hotkey hotkey, System.Threading.CancellationToken ct = default)
        {
            Registered = true;
            LastHotkey = hotkey;
            return Task.CompletedTask;
        }
        public Task UnregisterAsync(System.Threading.CancellationToken ct = default)
        {
            Registered = false;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SaveAddsSourceRootAndRegistersHotkey()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var fake = new FakeHotkeyService();
        var service = new SettingsService(fake, temp);
        var settings = new AppSettings { SourceRoot = "/data" };
        await service.SaveAsync(settings);

        Assert.Contains("/data", service.Current.WatchedRoots);
        Assert.True(fake.Registered);
        Assert.True(File.Exists(temp));
    }
}
