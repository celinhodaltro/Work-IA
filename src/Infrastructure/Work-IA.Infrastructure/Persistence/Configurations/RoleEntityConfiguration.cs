using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.TechnologiesJson).HasMaxLength(4000);
        builder.Property(e => e.MethodologiesJson).HasMaxLength(4000);
        builder.Property(e => e.ToolsJson).HasMaxLength(4000);
    }
}
