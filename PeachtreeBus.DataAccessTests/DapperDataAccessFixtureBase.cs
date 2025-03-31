using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Serialization;
using PeachtreeBus.Tests;
using PeachtreeBus.Tests.Fakes;
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
            FakeClock.Instance,
            TestDapperTypesHandler.Instance);
    }

    protected async Task Given_MessagesInQueue(int count)
    {
        Assert.AreEqual(0, CountRowsInTable(QueuePending));
        for (var i = 0; i < count; i++)
        {
            await dataAccess.AddMessage(
                TestData.CreateQueueMessage(
                    notBefore: DateTime.Now.AddMinutes(-1)),
                DefaultQueue);
        }
        Assert.AreEqual(count, CountRowsInTable(QueuePending));
    }
}
