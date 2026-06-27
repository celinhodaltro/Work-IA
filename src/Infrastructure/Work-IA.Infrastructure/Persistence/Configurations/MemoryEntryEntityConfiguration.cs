using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class MemoryEntryEntityConfiguration : IEntityTypeConfiguration<MemoryEntryEntity>
{
    public void Configure(EntityTypeBuilder<MemoryEntryEntity> builder)
    {
        builder.ToTable("MemoryEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.TagsJson).HasMaxLength(2000);
        builder.HasIndex(e => e.MemoryId).IsUnique();
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Score);
    }
}
