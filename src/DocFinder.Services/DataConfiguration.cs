using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DocFinder.Domain;

namespace DocFinder.Services;

/// <summary>EF Core configuration for <see cref="Data"/>.</summary>
public sealed class DataConfiguration : IEntityTypeConfiguration<Data>
{
    public void Configure(EntityTypeBuilder<Data> builder)
    {
        builder.HasKey(d => d.FileId);
        builder.Property(d => d.DataBytes).HasColumnType("BLOB");
    }
}
