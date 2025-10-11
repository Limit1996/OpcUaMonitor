using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpcUaMonitor.Domain.Ua;

namespace OpcUaMonitor.Infrastructure.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Opc_Channels");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(200).IsRequired();
        builder.Ignore(x => x.Tags);
        builder.HasMany(c => c.Devices).WithOne().OnDelete(DeleteBehavior.Cascade);
    }
}

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Opc_Devices");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasMany(c => c.Tags).WithOne().OnDelete(DeleteBehavior.Cascade);
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Opc_Tags");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Ignore(x => x.Value);
        builder.Property(x => x.DataType).HasMaxLength(50).IsRequired();
    }
}