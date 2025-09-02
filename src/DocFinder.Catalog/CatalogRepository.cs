using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using MetadataEntity = DocFinder.Domain.Metadata;
using DocFinder.Domain;

namespace DocFinder.Catalog;

public sealed class CatalogRepository
{
    private readonly DbContextOptions<CatalogDbContext> _options;

    public CatalogRepository(string? dbPath = null)
    {
        var path = dbPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocFinder", "catalog.db");
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var connectionString = $"Data Source={path}";
        _options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(connectionString)
            .Options;
        using var db = new CatalogDbContext(_options);
        db.Database.EnsureCreated();
    }

    public async Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default)
    {
        await using var db = new CatalogDbContext(_options);
        var entity = await db.Files.Include(f => f.Metadata)
            .FirstOrDefaultAsync(f => f.FileId == doc.FileId, ct);
        if (entity is null)
        {
            entity = new FileEntity { FileId = doc.FileId, Metadata = new MetadataEntity { FileId = doc.FileId } };
            db.Files.Add(entity);
        }
        else if (entity.Metadata is null)
        {
            entity.Metadata = new MetadataEntity { FileId = doc.FileId };
        }

        entity.FilePath = doc.Path;
        entity.Name = doc.FileName;
        entity.Ext = doc.Ext;
        entity.SizeBytes = doc.SizeBytes;
        entity.CreatedUtc = doc.CreatedUtc;
        entity.ModifiedUtc = doc.ModifiedUtc;
        entity.Sha256 = doc.Sha256;
        entity.Author = doc.Author ?? string.Empty;

        entity.Metadata.Version = doc.Version;
        entity.Metadata.CaseNumber = doc.CaseNumber;
        entity.Metadata.ParcelId = doc.ParcelId;
        entity.Metadata.Address = doc.Address;
        entity.Metadata.Tags = doc.Tags;

        await db.SaveChangesAsync(ct);
    }

    public async Task<DateTime?> GetLastModifiedUtcAsync(string path, CancellationToken ct = default)
    {
        await using var db = new CatalogDbContext(_options);
        var file = await db.Files.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FilePath == path, ct);
        return file?.ModifiedUtc;
    }

    public async Task<Guid?> DeleteFileAsync(string path, CancellationToken ct = default)
    {
        await using var db = new CatalogDbContext(_options);
        var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == path, ct);
        if (entity is null)
            return null;
        var fileId = entity.FileId;
        db.Files.Remove(entity);
        await db.SaveChangesAsync(ct);
        return fileId;
    }
}
