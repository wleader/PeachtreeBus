using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;
using Projects;

namespace PeachtreeBus.Example.AppHost;

[ExcludeFromCodeCoverage(Justification = "Code is an example.")]
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddProject<PeachtreeBus_Example_MicrosoftDI>("Example-Endpoint-MicrosoftDI")
            .WithExplicitStart();

        builder.Build().Run();
    }
}