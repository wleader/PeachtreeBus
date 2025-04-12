using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Management;

namespace PeachtreeBus.Core.Tests.Management;

[TestClass]
public class ManagementAccessFixture
{
    private ManagementDataAccess _dataAccess = default!;
    private readonly Mock<ISharedDatabase> _sharedDatabase = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IDapperTypesHandler> _dapperTypes = new();

    [TestInitialize]
    public void Initialize()
    {
        _dapperTypes.Reset();

        _dapperTypes.Setup(t => t.Configure())
            .Returns(true);

        _dataAccess = new(
            _sharedDatabase.Object,
            _busConfiguration.Object,
            FakeLog.Create<ManagementDataAccess>(),
            _dapperTypes.Object);
    }

    [TestMethod]
    public void Then_DapperIsConfigured()
    {
        _dapperTypes.Verify(t => t.Configure(), Times.Once());
        Assert.IsTrue(_dataAccess.DapperConfigured);
    }
}

