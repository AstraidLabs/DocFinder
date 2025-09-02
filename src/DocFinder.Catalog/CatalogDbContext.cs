using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;
using FileListEntity = DocFinder.Domain.FileList;
using FileListItemEntity = DocFinder.Domain.FileListItem;

namespace DocFinder.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<DataEntity> Data => Set<DataEntity>();
    public DbSet<FileListEntity> FileLists => Set<FileListEntity>();
    public DbSet<FileListItemEntity> FileListItems => Set<FileListItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
