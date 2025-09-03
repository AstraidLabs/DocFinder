using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocFinder.Services;

public sealed class ProtocolListConfiguration : IEntityTypeConfiguration<ProtocolList>
{
    public void Configure(EntityTypeBuilder<ProtocolList> b)
    {
        b.ToTable("ProtocolLists");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        b.Property(x => x.Owner).IsRequired().HasMaxLength(128);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CreatedUtc).IsRequired();
        b.Property(x => x.ModifiedUtc).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasMany<ProtocolListItem>()
         .WithOne()
         .HasForeignKey(i => i.ListId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProtocolListItemConfiguration : IEntityTypeConfiguration<ProtocolListItem>
{
    public void Configure(EntityTypeBuilder<ProtocolListItem> b)
    {
        b.ToTable("ProtocolListItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.RowVersion).IsRowVersion();
        b.Property(x => x.Order).IsRequired();
        b.Property(x => x.AddedUtc).IsRequired();

        b.Property(x => x.Label).HasMaxLength(256);
        b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.PinnedVersion).HasMaxLength(64);
        b.Property(x => x.PinnedFileSha256).HasMaxLength(64);

        // unikát: jeden Protocol jen jednou v daném seznamu
        b.HasIndex(x => new { x.ListId, x.ProtocolId }).IsUnique();

        // unikát: jedno pořadí per seznam
        b.HasIndex(x => new { x.ListId, x.Order }).IsUnique();

        // volitelná navigace na Protocol
        b.HasOne(x => x.Protocol)
         .WithMany()
         .HasForeignKey(x => x.ProtocolId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
