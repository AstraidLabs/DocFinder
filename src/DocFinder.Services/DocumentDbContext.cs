using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocFinder.Services;

public class DocumentDbContext : DbContext
{
    public DocumentDbContext()
    {
    }

    public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
        : base(options)
    {
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
}
