using DocFinder.Domain;
using DocFinder.Application;
using Microsoft.EntityFrameworkCore;

namespace DocFinder.Services;

public class DocumentDbContext : DbContext
{
    private readonly IDocumentIndexService? _index;

    public DocumentDbContext(IDocumentIndexService? index = null)
    {
        _index = index;
    }

    public DocumentDbContext(DbContextOptions<DocumentDbContext> options, IDocumentIndexService? index = null)
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

        optionsBuilder.AddInterceptors(new DocumentSaveChangesInterceptor(_index));
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
}
