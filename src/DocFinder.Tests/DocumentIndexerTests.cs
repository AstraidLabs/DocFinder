using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WordDoc = DocumentFormat.OpenXml.Wordprocessing.Document;
using DocFinder.Catalog;
using DocFinder.Domain.Settings;
using DocFinder.Indexing;
using DocFinder.Search;
using Lucene.Net.Store;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DocFinder.Services;
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

    private sealed class TestFactory : IDbContextFactory<DocumentDbContext>
    {
        private readonly DbContextOptions<DocumentDbContext> _options;

        public TestFactory(DbContextOptions<DocumentDbContext> options)
            => _options = options;

        public DocumentDbContext CreateDbContext() => new DocumentDbContext(_options);

        public ValueTask<DocumentDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => new ValueTask<DocumentDbContext>(new DocumentDbContext(_options));
    }

    [Fact(Skip = "Indexer disabled for protocol migration")]
    public async Task IndexFileStoresData()
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
        var dbPath = Path.Combine(temp, "catalog.db");
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        var factory = new TestFactory(options);
        var catalog = new CatalogRepository(factory);
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
        cmd.CommandText = "SELECT SizeBytes,Sha256 FROM Files WHERE FilePath=$p"; 
        cmd.Parameters.AddWithValue("$p", file);
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetInt64(0) > 0);
        Assert.False(string.IsNullOrEmpty(reader.GetString(1)));

    }

    [Fact(Skip = "Indexer disabled for protocol migration")]
    public async Task MissingFileRemovesRecord()
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
        var dbPath = Path.Combine(temp, "catalog.db");
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        var factory = new TestFactory(options);
        var catalog = new CatalogRepository(factory);
        using var search = new LuceneSearchService(new RAMDirectory());
        var extractors = new IContentExtractor[]
        {
            new DocxContentExtractor(),
            new PdfContentExtractor()
        };
        var indexer = new DocumentIndexer(search, catalog, settings, extractors);
        await indexer.IndexFileAsync(file);
        File.Delete(file);
        await indexer.IndexFileAsync(file);

        await using var connection = new SqliteConnection($"Data Source={Path.Combine(temp, "catalog.db")}");
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Files";
        var countObj = await cmd.ExecuteScalarAsync();
        var count = Convert.ToInt64(countObj);
        Assert.Equal(0, count);
    }
}
