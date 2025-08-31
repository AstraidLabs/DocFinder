using System;
using System.Collections.Generic;
using System.Linq;
using DocFinder.Domain;
using DocFinder.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite(connection)
            .Options;
        using var ctx = new DocumentDbContext(options, index);
        ctx.Database.EnsureCreated();

        var doc = new Document
        {
            BuildingName = "A",
            Name = "Doc",
            Author = "Auth",
            ModifiedAt = DateTime.UtcNow,
            Version = "1.0",
            Type = "txt",
            FileLink = "test.txt"
        };
        ctx.Documents.Add(doc);
        ctx.SaveChanges();

        Assert.Single(ctx.AuditEntries);
        Assert.Equal("Insert", ctx.AuditEntries.First().Action);
        Assert.Contains(doc.Id, index.Indexed);

        doc.Name = "Doc2";
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
