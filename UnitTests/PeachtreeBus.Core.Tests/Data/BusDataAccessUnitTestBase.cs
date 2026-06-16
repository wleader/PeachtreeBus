using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessUnitTestBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    protected TBusDataAccess DataAccess = default!;
    protected readonly Mock<IDapperMethods> DapperMethods = new();
    protected readonly Mock<IBusConfiguration> BusConfiguration = new();
    
    [TestInitialize]
    public void Initialize()
    {   
        BusConfiguration.Reset();
        DapperMethods.Reset();
        
        BusConfiguration.SetupGet(c => c.Schema)
            .Returns(new SchemaName("PBus"));
        
        DataAccess = CreateDataAccess();
    }

    protected abstract TBusDataAccess CreateDataAccess();
}