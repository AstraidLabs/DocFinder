using System.Threading;
using System.Threading.Tasks;

namespace DocFinder.Domain.Settings;

public interface ISettingsService
{
    AppSettings Current { get; }
    Task<AppSettings> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(AppSettings settings, CancellationToken ct = default);
}
