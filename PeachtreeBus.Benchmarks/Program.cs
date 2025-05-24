using BenchmarkDotNet.Running;
using PeachtreeBus.Benchmarks.TypeLookup;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage(Justification = "This is non-shipping benchmark code.")]
internal class Program
{
    private static void Main()
    {
        _ = BenchmarkRunner.Run<ClassNameServiceBenchmarks>();
    }
}