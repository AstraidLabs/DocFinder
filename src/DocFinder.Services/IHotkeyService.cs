using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain.Settings;

namespace DocFinder.Services;

public interface IHotkeyService
{
    Task RegisterAsync(Hotkey hotkey, CancellationToken ct = default);
    Task UnregisterAsync(CancellationToken ct = default);
}
