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
using File = DocFinder.Domain.File;

namespace DocFinder.Tests;

public class DocumentDbContextTests
{
    private sealed class StubIndex : ILuceneIndexService
    {
        public List<Guid> Indexed { get; } = new();
        public List<Guid> Deleted { get; } = new();
        public void IndexDocument(Protocol doc) => Indexed.Add(doc.Id);
        public void DeleteDocument(Guid docId) => Deleted.Add(docId);
        public IEnumerable<Protocol> Search(string query) => Enumerable.Empty<Protocol>();
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
        ctx.Database.Migrate();

        var fileId = Guid.NewGuid();
        var data = new Data(fileId, "v1", "text/plain", new byte[] { 1 });
        var file = new File(fileId, "test.txt", "test.txt", "txt", DateTime.UtcNow, "Auth", data);
        var doc = new Protocol(
            id: Guid.NewGuid(),
            file: file,
            title: "Doc",
            referenceNumber: "Ref",
            type: ProtocolType.Other,
            issueDateUtc: DateTime.UtcNow,
            issuedBy: "Auth",
            legalBasis: null,
            responsiblePerson: "Resp");
        ctx.Protocols.Add(doc);
        ctx.SaveChanges();

        Assert.Single(ctx.AuditEntries);
        Assert.Equal("Insert", ctx.AuditEntries.First().Action);
        Assert.Contains(doc.Id, index.Indexed);

        doc.SetTitle("Doc2");
        ctx.SaveChanges();
        Assert.Equal(2, ctx.AuditEntries.Count());
        Assert.Equal("Update", ctx.AuditEntries.OrderBy(a => a.Id).Last().Action);
        Assert.Equal(2, index.Indexed.Count);

        ctx.Protocols.Remove(doc);
        ctx.SaveChanges();
        Assert.Equal(3, ctx.AuditEntries.Count());
        Assert.Contains(doc.Id, index.Deleted);
    }
}
