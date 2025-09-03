using System;
using System.Linq;
using DocFinder.Domain;
using Xunit;
using FileEntity = DocFinder.Domain.File;
using ProtocolEntity = DocFinder.Domain.Protocol;

namespace DocFinder.Tests;

public class ProtocolListTests
{
    private static FileEntity CreateFile(byte[]? content = null)
    {
        var bytes = content ?? new byte[] { 1 };
        var data = new Data(Guid.NewGuid(), "v1", "application/octet-stream", bytes);
        return new FileEntity(Guid.NewGuid(), "a/b", "name", "txt", DateTime.UtcNow, "auth", data);
    }

    private static ProtocolEntity CreateProtocol(string? version = null, byte[]? content = null)
    {
        var file = CreateFile(content);
        var protocol = new ProtocolEntity(Guid.NewGuid(), file, "title", "ref", ProtocolType.Other,
            DateTime.UtcNow, "issuer", null, "resp");
        if (version is not null)
            protocol.SetVersion(version);
        return protocol;
    }

    [Fact]
    public void AddProtocols_AddsInOrderAndSkipsDuplicates()
    {
        var list = new ProtocolList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var a = CreateProtocol();
        var b = CreateProtocol();
        var c = CreateProtocol();

        list.AddProtocols(new[] { a, b, c });
        list.AddProtocols(new[] { a }); // duplicate

        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { a.Id, b.Id, c.Id }, list.Items.Select(i => i.ProtocolId));
    }

    [Fact]
    public void Reorder_MovesItemToNewIndex()
    {
        var list = new ProtocolList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var a = CreateProtocol();
        var b = CreateProtocol();
        var c = CreateProtocol();
        list.AddProtocols(new[] { a, b, c });

        list.Reorder(b.Id, 2);

        Assert.Equal(new[] { a.Id, c.Id, b.Id }, list.Items.Select(i => i.ProtocolId));
    }

    [Fact]
    public void PinAndUnpin_Works()
    {
        var list = new ProtocolList(Guid.NewGuid(), "list", DateTime.UtcNow, "owner");
        var protocol = CreateProtocol(version: "v1");
        list.AddProtocol(protocol, pinToProtocolVersion: true, pinToFileChecksum: true);

        var item = list.Items.Single();
        Assert.Equal("v1", item.PinnedVersion);
        Assert.Equal(protocol.File.Sha256, item.PinnedFileSha256);

        list.UnpinVersion(protocol.Id);
        list.UnpinFileChecksum(protocol.Id);

        item = list.Items.Single();
        Assert.Null(item.PinnedVersion);
        Assert.Null(item.PinnedFileSha256);
    }
}
