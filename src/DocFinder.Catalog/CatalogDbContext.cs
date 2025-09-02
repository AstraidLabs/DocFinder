using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;

namespace DocFinder.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<DataEntity> Data => Set<DataEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>()
            .HasKey(f => f.FileId);

        modelBuilder.Entity<DataEntity>()
            .HasKey(d => d.IdData);

        modelBuilder.Entity<FileEntity>()
            .HasOne(f => f.Data)
            .WithOne(d => d.File)
            .HasForeignKey<DataEntity>(d => d.FileId)
            .IsRequired();
    }
}
