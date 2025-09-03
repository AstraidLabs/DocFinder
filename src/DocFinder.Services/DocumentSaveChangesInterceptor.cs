using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.Domain;
using DocFinder.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DocFinder.Services;

public sealed class DocumentSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ILuceneIndexService? _index;
    private readonly IDbContextFactory<DocumentDbContext> _factory;
    private List<(Protocol doc, string action)> _changes = new();

    public DocumentSaveChangesInterceptor(IDbContextFactory<DocumentDbContext> factory, ILuceneIndexService? index)
    {
        _factory = factory;
        _index = index;
    }

    private static List<(Protocol doc, string action)> GetDocumentChanges(DbContext context)
    {
        var list = new List<(Protocol, string)>();
        foreach (var entry in context.ChangeTracker.Entries<Protocol>())
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

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            _changes = GetDocumentChanges(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            _changes = GetDocumentChanges(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context != null)
        {
            PostSave(eventData.Context).GetAwaiter().GetResult();
        }
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            await PostSave(eventData.Context, cancellationToken);
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PostSave(DbContext context, CancellationToken ct = default)
    {
        if (_changes.Count == 0)
        {
            return;
        }

        var changes = _changes;
        _changes = new();

        var audits = changes
            .Select(c => new AuditEntry(c.doc.Id, c.action, DateTime.UtcNow, Environment.UserName))
            .ToList();

        await using var auditCtx = _factory.CreateDbContext();
        await auditCtx.AddRangeAsync(audits, ct);
        await auditCtx.SaveChangesAsync(ct);

        if (_index != null)
        {
            foreach (var c in changes)
            {
                if (c.action == "Delete")
                {
                    _index.DeleteDocument(c.doc.Id);
                }
                else
                {
                    _index.IndexDocument(c.doc);
                }
            }
        }
    }
}
