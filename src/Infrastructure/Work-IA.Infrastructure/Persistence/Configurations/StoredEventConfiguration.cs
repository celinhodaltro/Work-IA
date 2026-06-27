using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Work_IA.Infrastructure.Persistence.Configurations;

public sealed class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("EventStore_StoredEvents");
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.EventType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.AggregateType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Data).IsRequired();
        builder.Property(e => e.OccurredOn).IsRequired();
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.AggregateId);
        builder.HasIndex(e => e.OccurredOn);
    }
}
