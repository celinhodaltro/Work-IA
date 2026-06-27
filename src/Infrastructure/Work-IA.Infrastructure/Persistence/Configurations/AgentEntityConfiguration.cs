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
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.CareerLevel).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.ExperiencePoints).HasDefaultValue(0);
        builder.Property(e => e.SkillsJson).HasColumnType("TEXT");
        builder.Property(e => e.RoleId).IsRequired(false);
    }
}
