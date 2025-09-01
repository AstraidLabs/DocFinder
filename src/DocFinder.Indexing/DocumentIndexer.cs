using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Search;
using DocFinder.Catalog;

namespace DocFinder.Indexing;

public sealed class DocumentIndexer : IIndexer
{
    private readonly ISearchService _search;
    private readonly CatalogRepository _catalog;
    private readonly ISettingsService _settings;
    private readonly IEnumerable<IContentExtractor> _extractors;
    private IndexingState _state = IndexingState.Indexing;

    public DocumentIndexer(
        ISearchService search,
        CatalogRepository catalog,
        ISettingsService settings,
        IEnumerable<IContentExtractor> extractors)
    {
        _search = search;
        _catalog = catalog;
        _settings = settings;
        _extractors = extractors;
    }

    public async Task IndexFileAsync(string path, CancellationToken ct = default)
    {
        if (_state == IndexingState.Paused)
            return;

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            return;

        var ext = fileInfo.Extension.Trim('.').ToLowerInvariant();

        string content = string.Empty;
        string? author = null;
        string? version = null;
        DateTime created = fileInfo.CreationTimeUtc;
        DateTime modified = fileInfo.LastWriteTimeUtc;

        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(ext));
        ExtractedContent meta;
        if (extractor is null)
        {
            meta = new ExtractedContent(string.Empty, null, null, null, null);
        }
        else
        {
            meta = await extractor.ExtractAsync(path, ct);
        }
        content = meta.Content;
        author = meta.Author;
        version = meta.Version;
        if (meta.Created.HasValue) created = meta.Created.Value.UtcDateTime;
        if (meta.Modified.HasValue) modified = meta.Modified.Value.UtcDateTime;

        var sha = ComputeSha256(path);
        var fileId = ComputeFileId(path, sha);
        var metaDict = new Dictionary<string,string>();
        if (!string.IsNullOrEmpty(author)) metaDict["author"] = author;
        if (!string.IsNullOrEmpty(version)) metaDict["version"] = version;

        var doc = new IndexDocument(
            fileId,
            path,
            fileInfo.Name,
            ext,
            fileInfo.Length,
            created,
            modified,
            sha,
            author,
            version,
            content,
            metaDict);
        await _search.IndexAsync(doc, ct);
        await _catalog.UpsertFileAsync(doc, ct);
    }

    public async Task ReindexAllAsync(CancellationToken ct = default)
    {
        foreach (var root in _settings.Current.WatchedRoots)
        {
            ct.ThrowIfCancellationRequested();
            if (!Directory.Exists(root)) continue;
            try
            {
                foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    var ext = Path.GetExtension(file).Trim('.').ToLowerInvariant();
                    if (_extractors.Any(e => e.CanHandle(ext)))
                    {
                        try
                        {
                            await IndexFileAsync(file, ct);
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }
    }

    public void Pause() => _state = IndexingState.Paused;

    public void Resume() => _state = IndexingState.Indexing;

    public IndexingState State => _state;

    private static string ComputeSha256(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Failed to compute SHA256 for {path}: {ex.Message}");
        }
        return string.Empty;
    }

    private static Guid ComputeFileId(string path, string sha)
    {
        var bytes = Encoding.UTF8.GetBytes(path + sha);
        var hash = SHA256.HashData(bytes);
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
