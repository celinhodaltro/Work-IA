using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class TokenUsageEntityConfiguration : IEntityTypeConfiguration<TokenUsageEntity>
{
    public void Configure(EntityTypeBuilder<TokenUsageEntity> builder)
    {
        builder.ToTable("TokenUsages");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.AgentId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.AgentId, e.Timestamp });
        builder.Property(e => e.ModelName).HasMaxLength(100);
        builder.Property(e => e.Cost).HasColumnType("decimal(18,6)");
    }
}
