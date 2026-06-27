using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class WorkflowInstanceEntityConfiguration : IEntityTypeConfiguration<WorkflowInstanceEntity>
{
    public void Configure(EntityTypeBuilder<WorkflowInstanceEntity> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.WorkflowName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.TriggerEventId).HasMaxLength(200);
    }
}
