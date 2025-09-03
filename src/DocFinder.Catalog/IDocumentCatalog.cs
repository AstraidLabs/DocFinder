using System;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;

namespace DocFinder.Catalog;

public interface IDocumentCatalog
{
    Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default);
    Task<DateTime?> GetLastModifiedUtcAsync(string path, CancellationToken ct = default);
    Task<Guid?> DeleteFileAsync(string path, CancellationToken ct = default);
}
