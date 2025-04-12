using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class DapperDataAccessFixtureBase : FixtureBase<DapperDataAccess>
{
    protected override DapperDataAccess CreateDataAccess()
    {
        return new DapperDataAccess(
            SharedDB,
            Configuration.Object,
            MockLog.Object,
            new FakeClock(),
            TestDapperTypesHandler.Instance);
    }

    protected async Task Given_MessagesInQueue(int count)
    {
        Assert.AreEqual(0, CountRowsInTable(QueuePending));
        for (var i = 0; i < count; i++)
        {
            await dataAccess.AddMessage(
                TestData.CreateQueueData(
                    notBefore: DateTime.Now.AddMinutes(-1)),
                DefaultQueue);
        }
        Assert.AreEqual(count, CountRowsInTable(QueuePending));
    }
}
