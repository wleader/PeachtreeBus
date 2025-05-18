using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;


[Generator]
public class LoggerExtensionsGenerator : GeneratorFromDI<ILoggerExtensionsFromXml>;

public interface ILoggerExtensionsFromXml : IIncrementalGenerator;

public class LoggerExtensionsFromXml(
    IGenerateFromXml generator)
    : ILoggerExtensionsFromXml
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith("LogMessages.xml", StringComparison.InvariantCultureIgnoreCase))
            .Select((a, c) => (a.Path, a.GetText(c)!.ToString()));

        var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());

        context.RegisterSourceOutput(compilationAndFiles, Generate);
    }

    private void Generate(SourceProductionContext context, (Compilation Left, ImmutableArray<(string Path, string)> Right) tuple)
    {
        var (_, files) = tuple;

        foreach (var file in files)
        {
            var (_, content) = file;
            try
            {
                var generated = generator.FromXml(content);
                context.AddSource("LogMessages.xml.g.cs", generated);
            }
            catch (Exception ex)
            {
                var descriptor = new DiagnosticDescriptor(
                    id: "PBSGEX01",
                    title: "Unhandled Exception",
                    messageFormat: "There was an unhandled exception when running the LoggerExtensions source generator. {0}",
                    category: "Category",
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                var instancce = Diagnostic.Create(descriptor, null, ex);
                context.ReportDiagnostic(instancce);
            }
        }
    }
}


