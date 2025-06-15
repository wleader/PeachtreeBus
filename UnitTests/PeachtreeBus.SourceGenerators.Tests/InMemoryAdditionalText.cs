using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading;

namespace PeachtreeBus.SourceGenerators.Tests
{
    internal class InMemoryAdditionalText(string path, string content) : AdditionalText
    {
        private readonly SourceText _content = SourceText.From(content, Encoding.UTF8);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => _content;
    }
}
