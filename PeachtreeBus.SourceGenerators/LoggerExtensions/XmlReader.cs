using System.IO;
using System.Xml.Serialization;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IXmlReader
{
    AssemblyType LoadXml(string xml);
}

public class XmlReader : IXmlReader
{
    public AssemblyType LoadXml(string xml)
    {
        var serializer = new XmlSerializer(typeof(AssemblyType));
        using var reader = new StringReader(xml);
        return (AssemblyType)serializer.Deserialize(reader);
    }
}
