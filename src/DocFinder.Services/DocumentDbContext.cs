using DocFinder.Domain;
using DocFinder.Search;
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

        var factory = new SimpleFactory((DbContextOptions<DocumentDbContext>)optionsBuilder.Options, _index);
        optionsBuilder.AddInterceptors(new DocumentSaveChangesInterceptor(factory, _index));
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    private sealed class SimpleFactory : IDbContextFactory<DocumentDbContext>
    {
        private readonly DbContextOptions<DocumentDbContext> _options;
        private readonly ILuceneIndexService? _index;
        public SimpleFactory(DbContextOptions<DocumentDbContext> options, ILuceneIndexService? index)
        {
            _options = options;
            _index = index;
        }
        public DocumentDbContext CreateDbContext() => new DocumentDbContext(_options, _index);
    }
}
