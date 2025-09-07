using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocFinder.Domain;
using DocFinder.Search;
using Lucene.Net.Store;
using Xunit;

namespace DocFinder.Tests;

public class LuceneSearchServiceTests
{
    [Fact]
    public async Task IndexAndQueryReturnsDocument()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        var id = Guid.NewGuid();
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello world", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("hello"));
        Assert.Equal(1, result.Total);
        Assert.Equal(id, result.Hits[0].FileId);
        Assert.Contains("<strong>hello</strong>", result.Hits[0].Snippet, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FuzzyQueryFindsResult()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        var id = Guid.NewGuid();
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("helo") { UseFuzzy = true });
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task CzechDiacriticsAreFolded()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        var id = Guid.NewGuid();
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "Příliš žluťoučký kůň", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("prilis zlutoucky kun"));
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task QueryHonorsPageSize()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        for (var i = 0; i < 5; i++)
        {
            await service.IndexAsync(new IndexDocument(Guid.NewGuid(), $"/{i}.txt", $"{i}.txt", "txt", 1, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello", new Dictionary<string, string>()));
        }

        var result = await service.QueryAsync(new UserQuery("hello") { PageSize = 2 });

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Hits.Count);
    }

    [Fact]
    public async Task QueryWithMultipleTypesFiltersResults()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        await service.IndexAsync(new IndexDocument(Guid.NewGuid(), "/a.pdf", "a.pdf", "pdf", 1, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello", new Dictionary<string, string>()));
        await service.IndexAsync(new IndexDocument(Guid.NewGuid(), "/b.docx", "b.docx", "docx", 1, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello", new Dictionary<string, string>()));
        await service.IndexAsync(new IndexDocument(Guid.NewGuid(), "/c.txt", "c.txt", "txt", 1, DateTime.UtcNow, DateTime.UtcNow, "hash", null, null, "hello", new Dictionary<string, string>()));

        var query = new UserQuery("hello")
        {
            Filters = new Dictionary<string, string> { ["type"] = "pdf,docx" }
        };

        var result = await service.QueryAsync(query);

        Assert.Equal(2, result.Total);
        Assert.All(result.Hits, h => Assert.True(h.Ext == "pdf" || h.Ext == "docx"));
    }
}
