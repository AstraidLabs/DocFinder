using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocFinder.Services;

public class DocumentDbContext : DbContext
{
    private readonly ILuceneIndexService? _index;

    public DocumentDbContext(ILuceneIndexService? index = null)
    {
        _index = index;
    }

    public DocumentDbContext(DbContextOptions<DocumentDbContext> options, ILuceneIndexService? index = null)
        : base(options)
    {
        _index = index;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=documents.db");
        }
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public override int SaveChanges()
    {
        var changes = GetDocumentChanges();
        var result = base.SaveChanges();
        PostSave(changes);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = GetDocumentChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await PostSaveAsync(changes, cancellationToken);
        return result;
    }

    private List<(Document doc, string action)> GetDocumentChanges()
    {
        var list = new List<(Document, string)>();
        foreach (var entry in ChangeTracker.Entries<Document>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    list.Add((entry.Entity, "Insert"));
                    break;
                case EntityState.Modified:
                    list.Add((entry.Entity, "Update"));
                    break;
                case EntityState.Deleted:
                    list.Add((entry.Entity, "Delete"));
                    break;
            }
        }
        return list;
    }

    private void PostSave(List<(Document doc, string action)> changes)
    {
        if (changes.Count == 0) return;
        var audits = changes
            .Select(c => new AuditEntry(c.doc.Id, c.action, DateTime.UtcNow, Environment.UserName))
            .ToList();
        AuditEntries.AddRange(audits);
        base.SaveChanges();
        if (_index != null)
        {
            foreach (var c in changes)
            {
                if (c.action == "Delete")
                    _index.DeleteDocument(c.doc.Id);
                else
                    _index.IndexDocument(c.doc);
            }
        }
    }

    private async Task PostSaveAsync(List<(Document doc, string action)> changes, CancellationToken ct)
    {
        if (changes.Count == 0) return;
        var audits = changes
            .Select(c => new AuditEntry(c.doc.Id, c.action, DateTime.UtcNow, Environment.UserName))
            .ToList();
        await AuditEntries.AddRangeAsync(audits, ct);
        await base.SaveChangesAsync(ct);
        if (_index != null)
        {
            foreach (var c in changes)
            {
                if (c.action == "Delete")
                    _index.DeleteDocument(c.doc.Id);
                else
                    _index.IndexDocument(c.doc);
            }
        }
    }
}
