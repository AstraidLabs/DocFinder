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

        optionsBuilder.AddInterceptors(new DocumentSaveChangesInterceptor(_index));
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
}
