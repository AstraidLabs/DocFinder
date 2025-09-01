using DocFinder.Domain;
using DocFinder.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

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

        // Ensure the options are strongly typed to DocumentDbContext. When the context
        // is constructed using the parameterless constructor (e.g. design-time tools),
        // optionsBuilder is non-generic and its Options property is untyped. Attempting
        // to cast in that scenario causes an invalid cast exception. Instead, use the
        // typed options when available or create a new typed builder.
        var sqliteExt = optionsBuilder.Options.FindExtension<SqliteOptionsExtension>();
        var builder = new DbContextOptionsBuilder<DocumentDbContext>();
        if (sqliteExt?.Connection != null)
        {
            builder.UseSqlite(sqliteExt.Connection);
        }
        else if (!string.IsNullOrEmpty(sqliteExt?.ConnectionString))
        {
            builder.UseSqlite(sqliteExt.ConnectionString!);
        }
        else
        {
            builder.UseSqlite("Data Source=documents.db");
        }

        var factory = new SimpleFactory(builder.Options, _index);
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
