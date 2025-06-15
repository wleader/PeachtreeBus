using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class DapperDataAccessFixture
{
    private DapperDataAccess _dataAccess = default!;
    private readonly Mock<ISharedDatabase> _sharedDatabase = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly FakeClock _clock = new();
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
            FakeLog.Create<DapperDataAccess>(),
            _clock,
            _dapperTypes.Object);
    }

    [TestMethod]
    public void Then_DapperIsConfigured()
    {
        _dapperTypes.Verify(t => t.Configure(), Times.Once());
        Assert.IsTrue(_dataAccess.DapperConfigured);
    }
}
