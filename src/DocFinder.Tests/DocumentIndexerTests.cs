using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WordDoc = DocumentFormat.OpenXml.Wordprocessing.Document;
using DocFinder.Catalog;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Indexing;
using DocFinder.Search;
using Lucene.Net.Store;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DocFinder.Tests;

public class DocumentIndexerTests
{
    private sealed class FakeSettingsService : ISettingsService
    {
        public FakeSettingsService(string root)
        {
            Current = new AppSettings { WatchedRoots = { root } };
        }
        public AppSettings Current { get; }
        public Task<AppSettings> LoadAsync(System.Threading.CancellationToken ct = default) => Task.FromResult(Current);
        public Task SaveAsync(AppSettings settings, System.Threading.CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task IndexFileStoresMetadata()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(temp);
        var file = Path.Combine(temp, "test.docx");
        using (var doc = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new WordDoc(new Body(new Paragraph(new Run(new Text("hello")))));
            main.Document.Save();
        }

        var settings = new FakeSettingsService(temp);
        var catalog = new CatalogRepository(Path.Combine(temp, "catalog.db"));
        using var search = new LuceneSearchService(new RAMDirectory());
        var extractors = new IContentExtractor[]
        {
            new DocxContentExtractor(),
            new PdfContentExtractor()
        };
        var indexer = new DocumentIndexer(search, catalog, settings, extractors);
        await indexer.IndexFileAsync(file);

        await using var connection = new SqliteConnection($"Data Source={Path.Combine(temp, "catalog.db")}");
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT SizeBytes,Sha256 FROM Files WHERE Path=$p";
        cmd.Parameters.AddWithValue("$p", file);
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetInt64(0) > 0);
        Assert.False(string.IsNullOrEmpty(reader.GetString(1)));
    }
}
