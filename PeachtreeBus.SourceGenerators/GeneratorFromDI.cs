using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.SourceGenerators;

/// <summary>
/// An incremental generator that initializes its self from the DI Container.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class GeneratorFromDI<TGenerator> : IIncrementalGenerator
    where TGenerator : class, IIncrementalGenerator
{
    private readonly TGenerator _generator = GeneratorComponents.Container.GetInstance<TGenerator>();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _generator.Initialize(context);
    }
}

