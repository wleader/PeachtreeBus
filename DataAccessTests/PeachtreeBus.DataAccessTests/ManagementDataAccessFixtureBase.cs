using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Management;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    public abstract class ManagementDataAccessFixtureBase : FixtureBase<ManagementDataAccess>
    {
        protected DapperDataAccess BusAccess = default!;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        protected override ManagementDataAccess CreateDataAccess()
        {
            BusAccess = new(
                SharedDB,
                Configuration.Object,
                new Mock<ILogger<DapperDataAccess>>().Object,
                DapperMethods);

            return new ManagementDataAccess(
                SharedDB,
                Configuration.Object,
                MockLog.Object,
                TestDapperTypesHandler.Instance);
        }

        protected async Task<SubscribedData> CreatePendingSubscribed()
        {
            var message = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(message);
            await Task.Delay(10); // make sure that messages get sequential enqueued times.
            return message;
        }

        protected async Task<SubscribedData> CreateFailedSubscribed()
        {
            var message = await CreatePendingSubscribed();
            await BusAccess.FailMessage(message);
            return message;
        }

        protected async Task<SubscribedData> CreateCompletedSubscribed()
        {
            var message = await CreatePendingSubscribed();
            await BusAccess.CompleteMessage(message);
            return message;
        }

        protected async Task<QueueData> CreatePendingQueued()
        {
            var message = TestData.CreateQueueData();
            message.Id = await BusAccess.AddMessage(message, DefaultQueue);
            await Task.Delay(10); // make sure that messages get sequential enqueued times.
            return message;
        }

        protected async Task<QueueData> CreateFailedQueued()
        {
            var message = await CreatePendingQueued();
            await BusAccess.FailMessage(message, DefaultQueue);
            return message;
        }

        protected async Task<QueueData> CreateCompletedQueued()
        {
            var message = await CreatePendingQueued();
            await BusAccess.CompleteMessage(message, DefaultQueue);
            return message;
        }

    }
}
