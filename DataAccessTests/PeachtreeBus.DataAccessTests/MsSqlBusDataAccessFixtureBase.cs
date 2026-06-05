using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class MsSqlBusDataAccessFixtureBase : FixtureBase<MsSqlBusDataAccess>
{
    protected override MsSqlBusDataAccess CreateDataAccess()
    {
        return new MsSqlBusDataAccess(
            SharedDB,
            Configuration.Object,
            MockLog.Object,
            DapperMethods,
            FakeBreakerProvider);
    }

    protected async Task Given_MessagesInQueue(int count)
    {
        Assert.AreEqual(0, CountRowsInTable(TestConfig.QueuePending));
        for (var i = 0; i < count; i++)
        {
            await dataAccess.AddMessage(
                TestData.CreateQueueData(
                    notBefore: DateTime.Now.AddMinutes(-1)),
                TestConfig.DefaultQueue);
        }
        Assert.AreEqual(count, CountRowsInTable(TestConfig.QueuePending));
    }
}
