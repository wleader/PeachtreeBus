using BenchmarkDotNet.Running;
using PeachtreeBus.Benchmarks.TypeLookup;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

internal class Program
{
    private static void Main(string[] args)
    {
        

        var summary = BenchmarkRunner.Run<ClassNameServiceBenchmarks>();
    }
}