namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IGenerateFromXml
{
    public string FromXml(string xmlContent);
}

public class GenerateFromXml(
    IXmlReader xmlReader,
    IAssemblyWriter genFromData)
    : IGenerateFromXml
{
    private readonly IXmlReader _xmlReader = xmlReader;
    private readonly IAssemblyWriter _genFromData = genFromData;

    public string FromXml(string xmlContent)
    {
        var data = _xmlReader.LoadXml(xmlContent);
        return _genFromData.Write(data);
    }
}
