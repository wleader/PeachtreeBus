using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class XmlReaderFixture
{
    private const string BasicXml =
        """
        <Assembly xmlns="http://tempuri.org/LogMessages.xsd" assemblyId="1" exludeFromCodeCoverage="false">
            <Usings>
                <Using>PeachtreeBus.Data</Using>
            </Usings>
            <Namespace name="PeachtreeBus.Data" namespaceId="2">
                <Class name="DapperDataAccess" classId="1">
                    <Event name="DataAccessError" level="Error" exception="true" eventId="1">
                        There was an exception interacting with the database. Method: {Method:string}.
                    </Event>
                </Class>
            </Namespace>
        </Assembly>
        """;

    [TestMethod]
    public void When_LoadXml_Then_ResultIsNotNull()
    {
        var reader = new XmlReader();
        var actual = reader.LoadXml(BasicXml);
        // just do some basic stuff. We aren't tring to test the whole serializer.
        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Usings.Length);
        Assert.AreEqual(1, actual.Namespace.Length);
    }
}
