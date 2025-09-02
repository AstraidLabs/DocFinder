using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using MetadataEntity = DocFinder.Domain.Metadata;

namespace DocFinder.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<MetadataEntity> Metadata => Set<MetadataEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>()
            .HasKey(f => f.FileId);

        modelBuilder.Entity<MetadataEntity>()
            .HasKey(m => m.FileId);

        modelBuilder.Entity<FileEntity>()
            .HasOne(f => f.Metadata)
            .WithOne(m => m.File)
            .HasForeignKey<MetadataEntity>(m => m.FileId)
            .IsRequired();
    }
}
