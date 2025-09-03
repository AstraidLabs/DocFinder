using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Catalog;

public sealed class CatalogRepository
{
    private readonly DbContextOptions<DocumentDbContext> _options;

    public CatalogRepository(string? dbPath = null)
    {
        var connectionString = dbPath != null
            ? $"Data Source={dbPath}"
            : DocumentDbContext.DefaultConnectionString;
        _options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite(connectionString)
            .Options;
        using var db = new DocumentDbContext(_options);
        db.Database.Migrate();
    }

    public async Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default)
    {
        await using var db = new DocumentDbContext(_options);
        var entity = await db.Files.Include(f => f.Data)
            .FirstOrDefaultAsync(f => f.FileId == doc.FileId, ct);
        var bytes = System.IO.File.ReadAllBytes(doc.Path);

        if (entity is null)
        {
            var data = new DataEntity(doc.FileId, doc.Version, doc.Ext, bytes);
            entity = new FileEntity(
                doc.FileId,
                doc.Path,
                doc.FileName,
                doc.Ext,
                doc.CreatedUtc,
                doc.Author ?? string.Empty,
                data);
            entity.Touch(doc.ModifiedUtc);
            db.Files.Add(entity);
        }
        else
        {
            entity.Move(doc.Path);
            entity.Rename(doc.FileName);
            entity.SetExt(doc.Ext);
            entity.SetAuthor(doc.Author ?? string.Empty);
            entity.SetCreated(doc.CreatedUtc);
            entity.ReplaceContent(bytes, doc.Ext, doc.Version);
            entity.Touch(doc.ModifiedUtc);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<DateTime?> GetLastModifiedUtcAsync(string path, CancellationToken ct = default)
    {
        await using var db = new DocumentDbContext(_options);
        var file = await db.Files.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FilePath == path, ct);
        return file?.ModifiedUtc;
    }

    public async Task<Guid?> DeleteFileAsync(string path, CancellationToken ct = default)
    {
        await using var db = new DocumentDbContext(_options);
        var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == path, ct);
        if (entity is null)
            return null;
        var fileId = entity.FileId;
        db.Files.Remove(entity);
        await db.SaveChangesAsync(ct);
        return fileId;
    }
}
