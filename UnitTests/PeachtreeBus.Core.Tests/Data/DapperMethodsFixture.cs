using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperMethodsFixture
{
    private DapperMethods _methods = default!;

    private readonly Mock<ISharedDatabase> _sharedDatabase = new();
    private readonly Mock<IDapperTypesHandler> _dapperTypes = new();

    [TestInitialize]
    public void Initialize()
    {
        _dapperTypes.Reset();
        _dapperTypes.Setup(t => t.Configure())
            .Returns(true);

        _methods = new(
            _dapperTypes.Object,
            _sharedDatabase.Object);
    }

    [TestMethod]
    public void Then_DapperIsConfigured()
    {
        _dapperTypes.Verify(t => t.Configure(), Times.Once());
        Assert.IsTrue(_methods.DapperConfigured);
    }
}
