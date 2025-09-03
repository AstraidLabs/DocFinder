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

public sealed class DocumentCatalog : IDocumentCatalog
{
    private readonly DbContextOptions<DocumentDbContext> _options;
    private readonly IMessageDialogService _dialogs;

    public DocumentCatalog(IMessageDialogService dialogs, string? dbPath = null)
    {
        _dialogs = dialogs;
        var connectionString = dbPath != null
            ? $"Data Source={dbPath}"
            : DocumentDbContext.DefaultConnectionString;
        _options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseSqlite(connectionString)
            .Options;
        try
        {
            using var db = new DocumentDbContext(_options);
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    public async Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default)
    {
        try
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
        catch (Exception ex) when (ex is IOException || ex is DbUpdateException)
        {
            _dialogs.ShowError($"Unable to update catalog for {doc.Path}: {ex.Message}");
            throw;
        }
    }

    public async Task<DateTime?> GetLastModifiedUtcAsync(string path, CancellationToken ct = default)
    {
        try
        {
            await using var db = new DocumentDbContext(_options);
            var file = await db.Files.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FilePath == path, ct);
            return file?.ModifiedUtc;
        }
        catch (Exception ex) when (ex is IOException || ex is DbUpdateException)
        {
            _dialogs.ShowError($"Unable to read catalog for {path}: {ex.Message}");
            throw;
        }
    }

    public async Task<Guid?> DeleteFileAsync(string path, CancellationToken ct = default)
    {
        try
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
        catch (Exception ex) when (ex is IOException || ex is DbUpdateException)
        {
            _dialogs.ShowError($"Unable to delete {path} from catalog: {ex.Message}");
            throw;
        }
    }
}
