using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataEntity = DocFinder.Domain.Data;

namespace DocFinder.Catalog;

/// <summary>EF Core configuration for <see cref="DataEntity"/>.</summary>
public sealed class DataConfiguration : IEntityTypeConfiguration<DataEntity>
{
    public void Configure(EntityTypeBuilder<DataEntity> builder)
    {
        builder.HasKey(d => d.FileId);
        builder.Property(d => d.DataBytes).HasColumnType("BLOB");
    }
}
