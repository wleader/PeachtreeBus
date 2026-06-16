using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessThrownExceptionsFixtureBase<TBusDataAccess>
    : BusDataAccessUnitTestBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    protected class TestException : Exception;

    protected readonly TestException ExpectedException = new();

    protected async Task Given_DapperWillThrow_When_Action_Then_Throws<T>(
        Func<Task> func)
    {
        Given_DapperThrows<T>();
        var thrown = await Assert.ThrowsExactlyAsync<TestException>(func);
        Assert.AreSame(ExpectedException, thrown);
    }

    protected void Given_DapperThrows<T>()
    {
        DapperMethods.Setup(m => m.QueryFirst<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(ExpectedException);
        DapperMethods.Setup(m => m.Query<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(ExpectedException);
        DapperMethods.Setup(m => m.QueryFirstOrDefault<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(ExpectedException);
        DapperMethods.Setup(m => m.ExecuteScalar<T>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(ExpectedException);
        DapperMethods.Setup(m => m.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(ExpectedException);
    }

    [TestMethod]
    public async Task Given_DapperWillThrow_When_AddMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<Identity>(() =>
            DataAccess.AddMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanQueueCompleted_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.CleanQueueCompleted(TestData.DefaultQueueName, TestData.Now, 10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanQueueFailed_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.CleanQueueFailed(TestData.DefaultQueueName, TestData.Now, 10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanSubscribedCompleted_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.CleanSubscribedCompleted(TestData.Now, 10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CleanSubscribedFailed_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.CleanSubscribedFailed(TestData.Now, 10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_CompleteMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<int>(() =>
            DataAccess.CompleteMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateSubscriptionMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<int>(() =>
            DataAccess.CompleteMessage(TestData.CreateSubscribedData()));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_DeleteSagaData_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<Identity>(() =>
            DataAccess.DeleteSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_EstimateQueuePending_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.EstimateQueuePending(TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_EstimateSubscribedPending_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.EstimateSubscribedPending(TestData.DefaultSubscriberId));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_ExpireSubscriptionMessages_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.ExpireSubscriptionMessages(10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_ExpireSubscriptions_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.ExpireSubscriptions(10));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_FailQueueMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<int>(() =>
            DataAccess.FailMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_FailSubscribedMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<int>(() =>
            DataAccess.FailMessage(TestData.CreateSubscribedData()));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetPendingQueued_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<QueueData>(() =>
            DataAccess.GetPendingQueued(TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetPendingSubscribed_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<SubscribedData>(() =>
            DataAccess.GetPendingSubscribed(TestData.DefaultSubscriberId));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_GetSagaData_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<SagaData>(() =>
            DataAccess.GetSagaData(TestData.DefaultSagaName, TestData.DefaultSagaKey));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_InsertSagaData_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<Identity>(() =>
            DataAccess.InsertSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_AddSubscribedMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<long>(() =>
            DataAccess.Publish(TestData.CreateSubscribedData(), TestData.DefaultTopic));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_Subscribe_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<int>(() =>
            DataAccess.Subscribe(TestData.DefaultSubscriberId, TestData.DefaultTopic, TestData.Now));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateQueueMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<QueueData>(() =>
            DataAccess.UpdateMessage(TestData.CreateQueueData(), TestData.DefaultQueueName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateSagaData_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<Identity>(() =>
            DataAccess.UpdateSagaData(TestData.CreateSagaData(), TestData.DefaultSagaName));

    [TestMethod]
    public async Task Given_DapperWillThrow_When_UpdateMSubscribedMessage_Then_Throws() =>
        await Given_DapperWillThrow_When_Action_Then_Throws<QueueData>(() =>
            DataAccess.UpdateMessage(TestData.CreateSubscribedData()));
}