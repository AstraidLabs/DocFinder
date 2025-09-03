using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DocFinder.Domain;
using FileEntity = DocFinder.Domain.File;

namespace DocFinder.Services;

/// <summary>EF Core configuration for <see cref="FileList"/>.</summary>
public sealed class FileListConfiguration : IEntityTypeConfiguration<FileList>
{
    public void Configure(EntityTypeBuilder<FileList> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Owner).IsRequired().HasMaxLength(128);

        builder.HasMany<FileListItem>()
               .WithOne()
               .HasForeignKey(i => i.ListId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core configuration for <see cref="FileListItem"/>.</summary>
public sealed class FileListItemConfiguration : IEntityTypeConfiguration<FileListItem>
{
    public void Configure(EntityTypeBuilder<FileListItem> itemBuilder)
    {
        itemBuilder.HasKey(i => i.Id);
        itemBuilder.Property(i => i.RowVersion).IsRowVersion();
        itemBuilder.HasIndex(i => new { i.ListId, i.FileId }).IsUnique();
        itemBuilder.HasIndex(i => new { i.ListId, i.Order }).IsUnique();
        itemBuilder.Property(i => i.PinnedSha256).HasMaxLength(64);
        itemBuilder.Property(i => i.Label).HasMaxLength(256);
        itemBuilder.Property(i => i.Note).HasMaxLength(2000);

        itemBuilder.HasOne<FileEntity>()
                   .WithMany()
                   .HasForeignKey(i => i.FileId)
                   .OnDelete(DeleteBehavior.Restrict);
    }
}
