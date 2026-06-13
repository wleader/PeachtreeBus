using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Management;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    public abstract class MsSqlManagementDataAccessFixtureBase : FixtureBase<MsSqlManagementDataAccess>
    {
        protected MsSqlBusDataAccess BusAccess = default!;

        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();


        protected override MsSqlManagementDataAccess CreateDataAccess()
        {
            BusAccess = new(
                SharedDB,
                Configuration.Object,
                new Mock<ILogger<MsSqlBusDataAccess>>().Object,
                DapperMethods,
                FakeBreakerProvider);

            return new MsSqlManagementDataAccess(
                Configuration.Object,
                MockLog.Object,
                DapperMethods);
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
            message.Id = await BusAccess.AddMessage(message, TestConfig.DefaultQueue);
            await Task.Delay(10); // make sure that messages get sequential enqueued times.
            return message;
        }

        protected async Task<QueueData> CreateFailedQueued()
        {
            var message = await CreatePendingQueued();
            await BusAccess.FailMessage(message, TestConfig.DefaultQueue);
            return message;
        }
    }
}
