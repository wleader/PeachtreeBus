using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class TopicDataTypeHandlerFixture : TypeHandlerFixtureBase<Topic, TopicHandler>
{
    [TestMethod]
    [DataRow("1")]
    [DataRow("TOPIC")]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(string value)
    {
        VerifySetValue(new Topic(value), DbType.String, value);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("TOPIC")]
    public void Given_Value_When_Parse_Then_Result(string value)
    {
        VerifyParse(value, new Topic(value));
    }
}
