using Microsoft.EntityFrameworkCore;

namespace OpcUaMonitor.Infrastructure;

public class OpcDbContext : DbContext
{
    public OpcDbContext(DbContextOptions options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IOpcUaMonitorInfrastructureFlag).Assembly);
    }
}
public class OpcDbContextFactory
{
    public static OpcDbContext Create(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OpcDbContext>();
        optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=OpcUaDb;Integrated Security=True;TrustServerCertificate=True;");

        return new OpcDbContext(optionsBuilder.Options);
    }
}