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
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", "hello world", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("hello", false, null, null, null));
        Assert.Equal(1, result.Total);
        Assert.Equal(id, result.Hits[0].FileId);
        Assert.Contains("<strong>hello</strong>", result.Hits[0].Snippet, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FuzzyQueryFindsResult()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        var id = Guid.NewGuid();
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", "hello", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("helo", true, null, null, null));
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task CzechDiacriticsAreFolded()
    {
        using var service = new LuceneSearchService(new RAMDirectory());
        var id = Guid.NewGuid();
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", 5, DateTime.UtcNow, DateTime.UtcNow, "hash", "Příliš žluťoučký kůň", new Dictionary<string, string>()));
        var result = await service.QueryAsync(new UserQuery("prilis zlutoucky kun", false, null, null, null));
        Assert.Equal(1, result.Total);
    }
}
