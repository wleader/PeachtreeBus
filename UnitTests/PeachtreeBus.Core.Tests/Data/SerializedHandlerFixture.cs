using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class SerializedHandlerFixture
{
    private readonly SerializedHandler<SagaMetaData> _sagaMetaDataHandler = new(TestSerializer.Instance);
    private readonly SerializedHandler<Headers> _headersHandler = new(TestSerializer.Instance);

    [TestMethod]
    public void Given_Null_When_Parse_Then_MetaDataIsDefault()
    {
        Assert.AreEqual(default, _sagaMetaDataHandler.Parse(null!));
    }

    [TestMethod]
    public void Given_Empty_When_Parse_Then_MetaDataIsDefault()
    {
        Assert.AreEqual(default, _sagaMetaDataHandler.Parse(string.Empty));
    }

    [TestMethod]
    public void Given_Null_When_Parse_Then_HeadersAreNull()
    {
        Assert.IsNull(_headersHandler.Parse(null!));
    }

    [TestMethod]
    public void Given_Empty_When_Parse_Then_HeadersAreNull()
    {
        Assert.IsNull(_headersHandler.Parse(string.Empty));
    }
}
