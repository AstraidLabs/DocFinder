using Microsoft.EntityFrameworkCore;
using DocFinder.Domain;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;
using FileListEntity = DocFinder.Domain.FileList;
using FileListItemEntity = DocFinder.Domain.FileListItem;

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
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<DataEntity> Data => Set<DataEntity>();
    public DbSet<FileListEntity> FileLists => Set<FileListEntity>();
    public DbSet<FileListItemEntity> FileListItems => Set<FileListItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
