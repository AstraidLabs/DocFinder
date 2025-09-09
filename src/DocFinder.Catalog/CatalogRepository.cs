using DocFinder.Domain;
using DocFinder.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataEntity = DocFinder.Domain.Data;
using FileEntity = DocFinder.Domain.File;

namespace DocFinder.Catalog;

public sealed class CatalogRepository
{
    private readonly IDbContextFactory<DocumentDbContext> _dbFactory;

    public CatalogRepository(IDbContextFactory<DocumentDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // Vyhledej podle FileId nebo, u starých indexù, podle sha
        var entity = await db.Files.Include(f => f.Data)
            .FirstOrDefaultAsync(f => f.FileId == doc.FileId, ct)
            ?? await db.Files.Include(f => f.Data)
                 .FirstOrDefaultAsync(f => f.Sha256 == doc.Sha256, ct);

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.Path, ct);

        if (entity is null)
        {
            var data = new DataEntity(doc.FileId, doc.Version, doc.Ext, bytes);
            entity = new FileEntity(doc.FileId, doc.Path, doc.FileName, doc.Ext,
                                    doc.CreatedUtc, doc.Author ?? string.Empty, data);
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
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var file = await db.Files.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FilePath == path, ct);
        return file?.ModifiedUtc;
    }

    public async Task<Guid?> DeleteFileAsync(string path, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var entity = await db.Files.FirstOrDefaultAsync(f => f.FilePath == path, ct);
        if (entity is null) return null;

        var fileId = entity.FileId;
        db.Files.Remove(entity);
        await db.SaveChangesAsync(ct);
        return fileId;
    }
}
