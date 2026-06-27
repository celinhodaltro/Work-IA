using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class AgentEntityConfiguration : IEntityTypeConfiguration<AgentEntity>
{
    public void Configure(EntityTypeBuilder<AgentEntity> builder)
    {
        builder.ToTable("Agents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Role).IsRequired();
        builder.Property(e => e.Status).IsRequired();
    }
}
