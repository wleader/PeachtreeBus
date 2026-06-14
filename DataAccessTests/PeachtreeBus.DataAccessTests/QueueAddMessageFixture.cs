using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public abstract class QueueAddMessageFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();
    
    [TestMethod]
    public async Task AddMessage_StoresTheMessage()
    {
        var newMessage = TestData.CreateQueueData();

        Assert.AreEqual(0, await TestDataAccess.CountRowsInTable(TestConfig.QueuePending));

        newMessage.Id = await BusDataAccess.AddMessage(newMessage, TestConfig.DefaultQueue);

        Assert.IsTrue(newMessage.Id.Value > 0);

        var messages = await TestDataAccess.GetTableContent<QueueData>(TestConfig.QueuePending);
        Assert.AreEqual(1, messages.Count);

        DataAssert.AreEqual(newMessage, messages[0]);
    }
}