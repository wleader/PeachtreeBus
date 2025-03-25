using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<PeachtreeBus_Example>("Example-Endpoint");

builder.Build().Run();
