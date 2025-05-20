using PeachtreeBus.SourceGenerators.LoggerExtensions;
using SimpleInjector;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.SourceGenerators;

public class Components
{
    private static Container? _container;

    [ExcludeFromCodeCoverage]
    public static Container Container => _container ??= BuildContainer();

    public static Container BuildContainer()
    {
        var container = new Container();
        container.Register(typeof(IGenerateFromXml), typeof(GenerateFromXml), Lifestyle.Transient);
        container.Register(typeof(IXmlReader), typeof(XmlReader), Lifestyle.Transient);
        container.Register(typeof(IAssemblyWriter), typeof(AssemblyWriter), Lifestyle.Transient);
        container.Register(typeof(IAssemblyBlocks), typeof(AssemblyBlocks), Lifestyle.Transient);
        container.Register(typeof(INamespaceBlocks), typeof(NamespaceBlocks), Lifestyle.Transient);
        container.Register(typeof(INamespaceWriter), typeof(NamespaceWriter), Lifestyle.Transient);
        container.Register(typeof(IClassWriter), typeof(ClassWriter), Lifestyle.Transient);
        container.Register(typeof(IEventWriter), typeof(EventWriter), Lifestyle.Transient);
        container.Register(typeof(IEventBlocks), typeof(EventBlocks), Lifestyle.Transient);
        container.Register(typeof(IState), typeof(State), Lifestyle.Singleton);
        container.Register(typeof(IParameterParser), typeof(ParameterParser), Lifestyle.Transient);
        container.Register(typeof(IEventTypeParser), typeof(EventTypeParser), Lifestyle.Transient);
        container.Register(typeof(IMessageValidator), typeof(MessageValidator), Lifestyle.Transient);
        container.Register(typeof(ILoggerExtensionsFromXml), typeof(LoggerExtensionsFromXml), Lifestyle.Transient);
        container.Verify();
        return container;
    }
}

