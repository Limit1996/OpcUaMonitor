using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpcUaMonitor.Domain.Sys;

namespace OpcUaMonitor.Infrastructure.Sys;

public class ProcessConfiguration : IEntityTypeConfiguration<Process>
{
    public void Configure(EntityTypeBuilder<Process> builder)
    {
        builder.ToTable("Sys_Processes");
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.HasMany(p => p.Devices).WithOne().OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Sys_Devices");
        builder.Property(d => d.Code).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.IpAddress).IsRequired().HasMaxLength(50);

        builder.Property(d => d.Manufacturer).HasMaxLength(200);
        builder.Property(d => d.Specification).HasMaxLength(1000);

        builder.HasIndex(d => d.Code).IsUnique();
        builder.HasMany(d => d.Channels)
            .WithOne()
            .HasForeignKey("SysDeviceId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
