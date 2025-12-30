using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Opc_Events");
        builder.HasKey(c => c.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.Property(x => x.TagId).IsRequired();

        builder.Property(x => x.ChannelId).IsRequired();

        builder
            .HasOne(c => c.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId)
            .OnDelete(DeleteBehavior.NoAction);

        // 显式配置外键关系
        builder
            .HasOne(c => c.Tag)
            .WithMany()
            .HasForeignKey(e => e.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // 为外键创建索引
        builder.HasIndex(e => e.TagId).HasDatabaseName("IX_Opc_Events_TagId");

        // 可选：为活动事件创建过滤索引
        builder
            .HasIndex(e => new { e.TagId, e.IsActive })
            .HasDatabaseName("IX_Opc_Events_TagId_IsActive")
            .HasFilter("[IsActive] = 1");
    }
}

public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("Opc_EventLogs");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.EventId).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired().HasConversion(
            v => v.ToString(),
            v => v ?? string.Empty
        );
        builder.Property(x => x.Timestamp).IsRequired().HasDefaultValue(DateTime.Now);

        // builder.Ignore(x => x.Parameters);
        builder.Property(x => x.Parameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonSerializerOptions.Default)
                     ?? new Dictionary<string, object>()
            )
            .HasColumnType("nvarchar(max)")
            .HasColumnName("Parameters");


        builder
            .HasOne(c => c.Event)
            .WithMany()
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}