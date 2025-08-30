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
        await service.IndexAsync(new IndexDocument(id, "/file.pdf", "file.pdf", "pdf", "hello world", new Dictionary<string,string>(), DateTime.UtcNow, DateTime.UtcNow));
        var result = await service.QueryAsync(new UserQuery("hello", false, null, null, null));
        Assert.Equal(1, result.Total);
        Assert.Equal(id, result.Hits[0].FileId);
        Assert.Contains("<b>hello</b>", result.Hits[0].Snippet, StringComparison.OrdinalIgnoreCase);
    }
}
