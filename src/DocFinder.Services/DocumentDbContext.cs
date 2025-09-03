using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocFinder.Services;

public class DocumentDbContext : DbContext
{
    public const string DefaultConnectionString = "Data Source=documents.db";

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
            optionsBuilder.UseSqlite(DefaultConnectionString);
        }
    }

    public DbSet<Protocol> Protocols => Set<Protocol>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<ProtocolList> ProtocolLists => Set<ProtocolList>();
    public DbSet<ProtocolListItem> ProtocolListItems => Set<ProtocolListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
