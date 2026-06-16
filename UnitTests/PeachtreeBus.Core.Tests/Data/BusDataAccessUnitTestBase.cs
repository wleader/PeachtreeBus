using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessUnitTestBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    protected TBusDataAccess DataAccess = default!;
    protected readonly Mock<IDapperMethods> DapperMethods = new();
    protected readonly Mock<IBusConfiguration> BusConfiguration = new();
    protected readonly Mock<ISharedDatabase> SharedDatabase = new();
    protected readonly Mock<ILogger<TBusDataAccess>> Log = new();
    protected readonly FakeBreakerProvider BreakerProvider = new();
    
    [TestInitialize]
    public void Initialize()
    {   
        Log.Reset();
        BusConfiguration.Reset();
        DapperMethods.Reset();
        SharedDatabase.Reset();
        
        BusConfiguration.SetupGet(c => c.Schema)
            .Returns(new SchemaName("PBus"));
        
        DataAccess = CreateDataAccess();
    }

    protected abstract TBusDataAccess CreateDataAccess();
}