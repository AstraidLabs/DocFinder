using System;
using System.Linq;
using DocFinder.Domain;
using FileEntity = DocFinder.Domain.File;
using Xunit;

namespace DocFinder.Tests;

public class FileListTests
{
    private static FileEntity CreateFile(byte[]? content = null)
    {
        var bytes = content ?? new byte[] { 1 };
        var data = new Data(Guid.NewGuid(), "v1", "application/octet-stream", bytes);
        return new FileEntity(Guid.NewGuid(), "a/b", "name", "txt", DateTime.UtcNow, "auth", data);
    }

    [Fact]
    public void AddFiles_AddsInOrderAndSkipsDuplicates()
    {
        var list = new FileList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var a = CreateFile();
        var b = CreateFile();
        var c = CreateFile();

        list.AddFiles(new[] { a, b, c });
        list.AddFiles(new[] { a }); // duplicate

        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { a.FileId, b.FileId, c.FileId }, list.Items.Select(i => i.FileId));
    }

    [Fact]
    public void InsertFiles_InsertsAtIndex()
    {
        var list = new FileList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var a = CreateFile();
        var b = CreateFile();
        var c = CreateFile();
        list.AddFiles(new[] { a, b, c });

        var x = CreateFile();
        var y = CreateFile();
        list.InsertFiles(1, new[] { x, y });

        Assert.Equal(new[] { a.FileId, x.FileId, y.FileId, b.FileId, c.FileId },
            list.Items.Select(i => i.FileId));
    }

    [Fact]
    public void Reorder_MovesItemToNewIndex()
    {
        var list = new FileList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var a = CreateFile();
        var b = CreateFile();
        var c = CreateFile();
        list.AddFiles(new[] { a, b, c });

        list.Reorder(b.FileId, 2);

        Assert.Equal(new[] { a.FileId, c.FileId, b.FileId }, list.Items.Select(i => i.FileId));
    }

    [Fact]
    public void PinAndUnpin_Works()
    {
        var list = new FileList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var file = CreateFile();
        list.AddFiles(new[] { file });

        list.PinFile(file.FileId, file.Sha256);
        Assert.Equal(file.Sha256, list.Items.Single().PinnedSha256);

        list.UnpinFile(file.FileId);
        Assert.Null(list.Items.Single().PinnedSha256);
    }
}
