using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<PeachtreeBus_Example_SimpleInjector>("Example-Endpoint-SimpleInjector")
    .WithExplicitStart();

builder.AddProject<PeachtreeBus_Example_MicrosoftDI>("Example-Endpoint-MicrosoftDI")
    .WithExplicitStart();

builder.Build().Run();
