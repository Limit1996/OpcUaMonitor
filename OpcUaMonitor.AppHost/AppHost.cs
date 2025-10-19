var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OpcUaMonitor_Api>("opcuamonitor-api");

builder.Build().Run();
