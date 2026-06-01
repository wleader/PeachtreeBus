using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace PeachtreeBus.SourceGenerators;

/// <summary>
/// An incremental generator that initializes its self from the DI Container.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class GeneratorFromDependencyInjection<TGenerator> : IIncrementalGenerator
    where TGenerator : class, IIncrementalGenerator
{
    private readonly TGenerator _generator = GeneratorComponents.ServiceProvider.GetRequiredService<TGenerator>();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _generator.Initialize(context);
    }
}

