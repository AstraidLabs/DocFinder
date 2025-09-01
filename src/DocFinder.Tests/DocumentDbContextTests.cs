using System;
using System.Collections.Generic;
using System.Linq;
using DocFinder.Domain;
using DocFinder.Services;
using DocFinder.Search;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DocFinder.Tests;

public class DocumentDbContextTests
{
    private sealed class StubIndex : ILuceneIndexService
    {
        public List<int> Indexed { get; } = new();
        public List<int> Deleted { get; } = new();
        public void IndexDocument(Document doc) => Indexed.Add(doc.Id);
        public void DeleteDocument(int docId) => Deleted.Add(docId);
        public IEnumerable<Document> Search(string query) => Enumerable.Empty<Document>();
    }

    [Fact]
    public void SaveChanges_WritesAuditAndIndexes()
    {
        var index = new StubIndex();
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton<ILuceneIndexService>(index);
        services.AddDbContextFactory<DocumentDbContext>(o => o.UseSqlite(connection));
        services.AddScoped<DocumentSaveChangesInterceptor>();
        services.AddDbContext<DocumentDbContext>((sp, o) =>
        {
            o.UseSqlite(connection);
            o.AddInterceptors(sp.GetRequiredService<DocumentSaveChangesInterceptor>());
        });

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        using var ctx = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
        ctx.Database.EnsureCreated();

        var doc = new Document(
            id: 0,
            buildingName: "A",
            name: "Doc",
            author: "Auth",
            modifiedAt: DateTime.UtcNow,
            version: "1.0",
            type: "txt",
            issuedAt: null,
            validUntil: null,
            canPrint: false,
            isElectronic: false,
            fileLink: "test.txt");
        ctx.Documents.Add(doc);
        ctx.SaveChanges();

        Assert.Single(ctx.AuditEntries);
        Assert.Equal("Insert", ctx.AuditEntries.First().Action);
        Assert.Contains(doc.Id, index.Indexed);

        doc.UpdateName("Doc2");
        ctx.SaveChanges();
        Assert.Equal(2, ctx.AuditEntries.Count());
        Assert.Equal("Update", ctx.AuditEntries.OrderBy(a => a.Id).Last().Action);
        Assert.Equal(2, index.Indexed.Count);

        ctx.Documents.Remove(doc);
        ctx.SaveChanges();
        Assert.Equal(3, ctx.AuditEntries.Count());
        Assert.Contains(doc.Id, index.Deleted);
    }
}
