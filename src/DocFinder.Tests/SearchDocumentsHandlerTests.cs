using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Application.Commands;
using DocFinder.Application.Handlers;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using Xunit;

namespace DocFinder.Tests;

public class SearchDocumentsHandlerTests
{
    private sealed class FakeSearchService : ISearchService
    {
        public UserQuery? ReceivedQuery { get; private set; }

        public Task IndexAsync(IndexDocument doc, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteByFileIdAsync(Guid fileId, CancellationToken ct = default) => Task.CompletedTask;

        public ValueTask<SearchResult> QueryAsync(UserQuery query, CancellationToken ct = default)
        {
            ReceivedQuery = query;
            return new ValueTask<SearchResult>(new SearchResult(0, new List<SearchHit>(), new Dictionary<string, int>()));
        }

        public Task OptimizeAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public FakeSettingsService(int pageSize) => Current = new AppSettings { PageSize = pageSize };
        public AppSettings Current { get; private set; }
        public Task<AppSettings> LoadAsync(CancellationToken ct = default) => Task.FromResult(Current);
        public Task SaveAsync(AppSettings settings, CancellationToken ct = default)
        {
            Current = settings;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task HandlerUsesConfiguredPageSize()
    {
        var search = new FakeSearchService();
        var settings = new FakeSettingsService(7);
        var handler = new SearchDocumentsHandler(search, settings);

        await handler.HandleAsync(new SearchDocumentsCommand("q", new SearchFilter()), CancellationToken.None);

        Assert.Equal(7, search.ReceivedQuery?.PageSize);
    }

    [Fact]
    public async Task HandlerFiltersPdfAndDocxWhenAllRequested()
    {
        var search = new FakeSearchService();
        var settings = new FakeSettingsService(5);
        var handler = new SearchDocumentsHandler(search, settings);

        await handler.HandleAsync(new SearchDocumentsCommand("q", new SearchFilter("all")), CancellationToken.None);

        Assert.True(search.ReceivedQuery?.Filters.ContainsKey("type"));
        Assert.Equal("pdf,docx", search.ReceivedQuery?.Filters["type"]);
    }
}

