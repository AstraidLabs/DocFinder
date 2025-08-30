using System;
using System.Threading;
using System.Threading.Tasks;

namespace DocFinder.Catalog;

public interface ITextRepository
{
    Task<string?> GetContentAsync(Guid fileId, CancellationToken ct = default);
}
