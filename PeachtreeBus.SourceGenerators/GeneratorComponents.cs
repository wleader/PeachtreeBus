using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.SourceGenerators;

public static class GeneratorComponents
{
    private static ServiceProvider? _serviceProvider;

    [ExcludeFromCodeCoverage]
    public static ServiceProvider ServiceProvider => _serviceProvider ??= BuildServiceProvider();

    public static ServiceProvider BuildServiceProvider()
    {
        var container = new ServiceCollection();
        container.AddTransient(typeof(IGenerateFromXml), typeof(GenerateFromXml));
        container.AddTransient(typeof(IXmlReader), typeof(XmlReader));
        container.AddTransient(typeof(IAssemblyWriter), typeof(AssemblyWriter));
        container.AddTransient(typeof(IAssemblyBlocks), typeof(AssemblyBlocks));
        container.AddTransient(typeof(INamespaceBlocks), typeof(NamespaceBlocks));
        container.AddTransient(typeof(INamespaceWriter), typeof(NamespaceWriter));
        container.AddTransient(typeof(IClassWriter), typeof(ClassWriter));
        container.AddTransient(typeof(IEventWriter), typeof(EventWriter));
        container.AddTransient(typeof(IEventBlocks), typeof(EventBlocks));
        container.AddSingleton(typeof(IState), typeof(State));
        container.AddTransient(typeof(IParameterParser), typeof(ParameterParser));
        container.AddTransient(typeof(IEventTypeParser), typeof(EventTypeParser));
        container.AddTransient(typeof(IMessageValidator), typeof(MessageValidator));
        container.AddTransient(typeof(ILoggerExtensionsFromXml), typeof(LoggerExtensionsFromXml));

        return container.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}

