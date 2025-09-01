using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DocFinder.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogFile> Files => Set<CatalogFile>();
    public DbSet<FileMetadata> Metadata => Set<FileMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogFile>()
            .HasKey(f => f.FileId);

        modelBuilder.Entity<FileMetadata>()
            .HasKey(m => new { m.FileId, m.Key });

        modelBuilder.Entity<FileMetadata>()
            .HasOne(m => m.File)
            .WithMany(f => f.Metadata)
            .HasForeignKey(m => m.FileId);
    }
}

public class CatalogFile
{
    public Guid FileId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Ext { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public ICollection<FileMetadata> Metadata { get; set; } = new List<FileMetadata>();
}

public class FileMetadata
{
    public Guid FileId { get; set; }
    public CatalogFile File { get; set; } = null!;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
