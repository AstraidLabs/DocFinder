using System;
using DocFinder.Domain;
using FileEntity = DocFinder.Domain.File;
using Xunit;

namespace DocFinder.Tests;

public class FileTests
{
    private static FileEntity CreateFile(byte[]? bytes = null)
    {
        var content = bytes ?? new byte[] { 0x00 };
        var data = new Data(Guid.NewGuid(), "v1", "application/octet-stream", content);
        return new FileEntity(Guid.NewGuid(), "path/file.txt", "file", "TXT", DateTime.UtcNow, "author", data);
    }

    [Fact]
    public void Constructor_Throws_WhenCreatedNotUtc()
    {
        var data = new Data(Guid.NewGuid(), null, "text/plain", new byte[] { 1 });
        var nonUtc = DateTime.Now;
        Assert.Throws<ArgumentException>(() =>
            new FileEntity(Guid.NewGuid(), "a/b", "name", "txt", nonUtc, "auth", data));
    }

    [Fact]
    public void ReplaceContent_RecomputesHashAndSize()
    {
        var file = CreateFile(new byte[] { 1 });
        var originalHash = file.Sha256;
        var originalSize = file.SizeBytes;

        var newContent = new byte[] { 2, 3, 4 };
        file.ReplaceContent(newContent, "application/octet-stream", "v2");

        Assert.NotEqual(originalHash, file.Sha256);
        Assert.NotEqual(originalSize, file.SizeBytes);
        Assert.Equal(newContent.Length, file.SizeBytes);
        Assert.True(file.ModifiedUtc >= file.CreatedUtc);
    }

    [Fact]
    public void Normalizes_Extension_And_Path()
    {
        var file = CreateFile();
        file.SetExt(".PDF");
        file.Move("C\\temp\\FILE.TXT ");

        Assert.Equal("pdf", file.Ext);
        Assert.Equal("C/temp/FILE.TXT", file.FilePath);
    }
}
