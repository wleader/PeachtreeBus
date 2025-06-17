using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Management;

namespace PeachtreeBus.Core.Tests.Management;

[TestClass]
public class ManagementAccessFixture
{
    private ManagementDataAccess _dataAccess = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IDapperTypesHandler> _dapperTypes = new();
    private readonly Mock<IDapperMethods> _dapperMethods = new();

    [TestInitialize]
    public void Initialize()
    {
        _dapperTypes.Reset();

        _dapperTypes.Setup(t => t.Configure())
            .Returns(true);

        _dataAccess = new(
            _busConfiguration.Object,
            FakeLog.Create<ManagementDataAccess>(),
            _dapperMethods.Object);
    }

    [TestMethod]
    public void Then()
    {
        Assert.Inconclusive("Tests not written.");
    }
}

