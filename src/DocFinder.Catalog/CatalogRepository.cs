using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using DocFinder.Domain;

namespace DocFinder.Catalog;

public sealed class CatalogRepository
{
    private readonly string _connectionString;

    public CatalogRepository(string? dbPath = null)
    {
        var path = dbPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocFinder", "catalog.db");
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        _connectionString = $"Data Source={path}";
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Files(
            FileId TEXT PRIMARY KEY,
            Path TEXT NOT NULL,
            FileName TEXT NOT NULL,
            Ext TEXT NOT NULL,
            SizeBytes INTEGER NOT NULL,
            CreatedUtc TEXT NOT NULL,
            ModifiedUtc TEXT NOT NULL,
            Sha256 TEXT NOT NULL
        );";
        cmd.ExecuteNonQuery();

        // Ensure SizeBytes column exists for databases created before it was introduced
        using (var colCmd = connection.CreateCommand())
        {
            colCmd.CommandText = "PRAGMA table_info(Files);";
            using var reader = colCmd.ExecuteReader();
            var hasSize = false;
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (string.Equals(name, "SizeBytes", StringComparison.OrdinalIgnoreCase))
                {
                    hasSize = true;
                    break;
                }
            }

            if (!hasSize)
            {
                using var alter = connection.CreateCommand();
                alter.CommandText = "ALTER TABLE Files ADD COLUMN SizeBytes INTEGER NOT NULL DEFAULT 0;";
                alter.ExecuteNonQuery();
            }
        }

        using var meta = connection.CreateCommand();
        meta.CommandText = @"CREATE TABLE IF NOT EXISTS Metadata(
            FileId TEXT NOT NULL,
            Key TEXT NOT NULL,
            Value TEXT NOT NULL,
            PRIMARY KEY(FileId,Key)
        );";
        meta.ExecuteNonQuery();
    }

    public async Task UpsertFileAsync(IndexDocument doc, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT OR REPLACE INTO Files(FileId,Path,FileName,Ext,SizeBytes,CreatedUtc,ModifiedUtc,Sha256) VALUES($id,$path,$name,$ext,$size,$created,$mod,$sha);";
        cmd.Parameters.AddWithValue("$id", doc.FileId.ToString());
        cmd.Parameters.AddWithValue("$path", doc.Path);
        cmd.Parameters.AddWithValue("$name", doc.FileName);
        cmd.Parameters.AddWithValue("$ext", doc.Ext);
        cmd.Parameters.AddWithValue("$size", doc.SizeBytes);
        cmd.Parameters.AddWithValue("$created", doc.CreatedUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$mod", doc.ModifiedUtc.ToString("o"));
        cmd.Parameters.AddWithValue("$sha", doc.Sha256);
        await cmd.ExecuteNonQueryAsync(ct);

        var del = connection.CreateCommand();
        del.CommandText = "DELETE FROM Metadata WHERE FileId=$id";
        del.Parameters.AddWithValue("$id", doc.FileId.ToString());
        await del.ExecuteNonQueryAsync(ct);

        foreach (var kv in doc.Metadata)
        {
            var meta = connection.CreateCommand();
            meta.CommandText = "INSERT INTO Metadata(FileId,Key,Value) VALUES($id,$k,$v)";
            meta.Parameters.AddWithValue("$id", doc.FileId.ToString());
            meta.Parameters.AddWithValue("$k", kv.Key);
            meta.Parameters.AddWithValue("$v", kv.Value);
            await meta.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    public async Task<DateTime?> GetLastModifiedUtcAsync(string path, CancellationToken ct = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT ModifiedUtc FROM Files WHERE Path=$p";
        cmd.Parameters.AddWithValue("$p", path);
        var result = await cmd.ExecuteScalarAsync(ct) as string;
        return result != null ? DateTime.Parse(result).ToUniversalTime() : null;
    }
}

