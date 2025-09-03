using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using FileEntity = DocFinder.Domain.File;
using DataEntity = DocFinder.Domain.Data;

namespace DocFinder.Services;

/// <summary>EF Core configuration for <see cref="FileEntity"/>.</summary>
public sealed class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(f => f.FileId);
        builder.Property(f => f.RowVersion)
               .IsRowVersion()
               .HasDefaultValue(Array.Empty<byte>());

        builder.HasOne(f => f.Data)
               .WithOne(d => d.File)
               .HasForeignKey<DataEntity>(d => d.FileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.Sha256).IsUnique();
        builder.HasIndex(f => f.FilePath);
        builder.HasIndex(f => new { f.Name, f.Ext });
    }
}
