using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;
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
        var entity = await db.Files.Include(f => f.Data)
            .FirstOrDefaultAsync(f => f.FileId == doc.FileId, ct);
        var bytes = System.IO.File.ReadAllBytes(doc.Path);
        var md5 = Convert.ToHexString(MD5.HashData(bytes));

        if (entity is null)
        {
            var data = new DataEntity(doc.FileId, doc.Version, doc.Ext, bytes, md5);
            entity = new FileEntity(
                doc.FileId,
                doc.Path,
                doc.FileName,
                doc.Ext,
                doc.SizeBytes,
                doc.CreatedUtc,
                doc.ModifiedUtc,
                doc.Sha256,
                doc.Author ?? string.Empty,
                data);
            db.Files.Add(entity);
        }
        else
        {
            entity.UpdatePath(doc.Path);
            entity.UpdateName(doc.FileName);
            entity.SetExt(doc.Ext);
            entity.UpdateSize(doc.SizeBytes);
            entity.UpdateCreated(doc.CreatedUtc);
            entity.UpdateModified(doc.ModifiedUtc);
            entity.UpdateSha256(doc.Sha256);
            entity.SetAuthor(doc.Author ?? string.Empty);

            if (entity.Data is null)
            {
                var data = new DataEntity(doc.FileId, doc.Version, doc.Ext, bytes, md5);
                entity.SetData(data);
            }
            else
            {
                entity.Data.SetDataVersion(doc.Version);
                entity.Data.SetFileType(doc.Ext);
                entity.Data.UpdateBytes(bytes);
                entity.Data.SetMd5(md5);
            }
        }

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
