using System;
using System.IO;
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
        cmd.CommandText = "SELECT SizeBytes,Sha256 FROM Files WHERE FilePath=$p"; 
        cmd.Parameters.AddWithValue("$p", file);
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetInt64(0) > 0);
        Assert.False(string.IsNullOrEmpty(reader.GetString(1)));

        var expectedMd5 = Convert.ToHexString(MD5.HashData(System.IO.File.ReadAllBytes(file)));
        var md5Cmd = connection.CreateCommand();
        md5Cmd.CommandText = "SELECT d.Md5 FROM Data d JOIN Files f ON f.FileId = d.FileId WHERE f.FilePath=$p";
        md5Cmd.Parameters.AddWithValue("$p", file);
        var actualMd5Obj = await md5Cmd.ExecuteScalarAsync();
        var actualMd5 = actualMd5Obj as string;
        Assert.Equal(expectedMd5, actualMd5);
    }

    [Fact]
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
        var catalog = new CatalogRepository(Path.Combine(temp, "catalog.db"));
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
