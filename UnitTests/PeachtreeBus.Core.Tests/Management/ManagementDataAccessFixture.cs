using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.Management;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Management;

[TestClass]
public class ManagementAccessFixture
{
    private ManagementDataAccess _dataAccess = default!;
    private readonly Mock<IBusConfiguration> _busConfiguration = new();
    private readonly Mock<IDapperTypesHandler> _dapperTypes = new();
    private readonly Mock<IDapperMethods> _dapperMethods = new();

    private readonly SchemaName _schemaName = new("PBus");
    private readonly List<QueueData> _queryQueueDataResult = [];
    private readonly List<SubscribedData> _querySubscribedDataResult = [];

    public class TestException : Exception;

    [TestInitialize]
    public void Initialize()
    {
        _busConfiguration.Reset();
        _dapperTypes.Reset();
        _dapperMethods.Reset();

        _dapperTypes.Setup(t => t.Configure())
            .Returns(true);

        _queryQueueDataResult.Clear();
        _queryQueueDataResult.Add(TestData.CreateQueueData());
        _queryQueueDataResult.Add(TestData.CreateQueueData());
        _dapperMethods.Setup(d => d.Query<QueueData>(It.IsAny<string>(), It.IsAny<DynamicParameters>()))
            .ReturnsAsync(() => _queryQueueDataResult);

        _querySubscribedDataResult.Clear();
        _querySubscribedDataResult.Add(TestData.CreateSubscribedData());
        _querySubscribedDataResult.Add(TestData.CreateSubscribedData());
        _dapperMethods.Setup(d => d.Query<SubscribedData>(It.IsAny<string>(), It.IsAny<DynamicParameters>()))
            .ReturnsAsync(() => _querySubscribedDataResult);

        _busConfiguration.SetupGet(c => c.Schema).Returns(() => _schemaName);

        _dataAccess = new(
            _busConfiguration.Object,
            FakeLog.Create<ManagementDataAccess>(),
            _dapperMethods.Object);
    }

    private (string statement, DynamicParameters? parameters) GetArguments()
    {
        Assert.AreEqual(1, _dapperMethods.Invocations.Count);
        var statement = (string)_dapperMethods.Invocations[0].Arguments[0];
        var parameters = (DynamicParameters?)_dapperMethods.Invocations[0].Arguments[1];
        return (statement, parameters);
    }

    private static void AssertTable(string? statement, string expected)
    {
        Assert.IsNotNull(statement);
        Assert.IsTrue(statement.Contains(expected), $"Statement does not use the correct table name {expected}. Actual: {statement}");
    }

    private static void AssertSkipTake(DynamicParameters? parameters, int skip, int take)
    {
        Assert.IsNotNull(parameters);
        Assert.AreEqual(skip, parameters.Get<int>("@Skip"));
        Assert.AreEqual(take, parameters.Get<int>("@Take"));
    }

    private void Given_DapperMethodThrows()
    {
        _dapperMethods.Setup(x => x.Query<QueueData>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(x => x.Query<SubscribedData>(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
        _dapperMethods.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()))
            .ThrowsAsync(new TestException());
    }

    [TestMethod]
    public async Task When_GetFailedQueueMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetFailedQueueMessages(new("QueueName"), 0, 10);

        CollectionAssert.AreEqual(_queryQueueDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[QueueName_Failed]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task When_GetCompletedQueueMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetCompletedQueueMessages(new("QueueName"), 0, 10);

        CollectionAssert.AreEqual(_queryQueueDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[QueueName_Completed]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task When_GetPendingQueueMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetPendingQueueMessages(new("QueueName"), 0, 10);

        CollectionAssert.AreEqual(_queryQueueDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[QueueName_Pending]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetFailedQueueMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetFailedQueueMessages(new("QueueName"), 0, 10));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetCompletedQueueMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetCompletedQueueMessages(new("QueueName"), 0, 10));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetPendingQueueMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetPendingQueueMessages(new("QueueName"), 0, 10));
    }

    [TestMethod]
    public async Task When_GetFailedSubscribedMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetFailedSubscribedMessages(0, 10);

        CollectionAssert.AreEqual(_querySubscribedDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[Subscribed_Failed]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task When_GetCompletedSubscribedMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetCompletedSubscribedMessages(0, 10);

        CollectionAssert.AreEqual(_querySubscribedDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[Subscribed_Completed]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task When_GetPendingSubscribedMessages_Then_DapperMethodInvoked()
    {
        var actual = await _dataAccess.GetPendingSubscribedMessages(0, 10);

        CollectionAssert.AreEqual(_querySubscribedDataResult, actual);
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "FROM [PBus].[Subscribed_Pending]");
        AssertSkipTake(parameters, 0, 10);
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetFailedSubscribedMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetCompletedSubscribedMessages(0, 10));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetCompletedSubscribedMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetCompletedSubscribedMessages(0, 10));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_GetPendingSubscribedMessages_Then_Throws()
    {
        Given_DapperMethodThrows();
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.GetPendingSubscribedMessages(0, 10));
    }

    [TestMethod]
    public async Task When_RetryFailedQueueuMessage_Then_DapperMethodInvoked()
    {
        var id = new Identity(12);
        await _dataAccess.RetryFailedQueueMessage(new("QueueName"), id);
        _dapperMethods.Verify(d => d.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()), Times.Once());
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "INSERT INTO [PBus].[QueueName_Pending]");
        AssertTable(statement, "DELETE FROM [PBus].[QueueName_Failed]");
        Assert.IsNotNull(parameters);
        Assert.AreEqual(id, parameters.Get<Identity>("@Id"));
    }

    [TestMethod]
    public async Task When_RetryFailedSusbcribedMessage_Then_DapperMethodInvoked()
    {
        var id = new Identity(12);
        await _dataAccess.RetryFailedSubscribedMessage(id);
        _dapperMethods.Verify(d => d.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()), Times.Once());
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "INSERT INTO [PBus].[Subscribed_Pending]");
        AssertTable(statement, "DELETE FROM [PBus].[Subscribed_Failed]");
        Assert.IsNotNull(parameters);
        Assert.AreEqual(id, parameters.Get<Identity>("@Id"));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_RetryFailedQueueuMessage_Then_Throws()
    {
        Given_DapperMethodThrows();
        var id = new Identity(12);
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.RetryFailedQueueMessage(new("QueueName"), id));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_RetryFailedSubscribedMessage_Then_Throws()
    {
        Given_DapperMethodThrows();
        var id = new Identity(12);
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.RetryFailedSubscribedMessage(id));
    }

    [TestMethod]
    public async Task When_CancelPendingQueueuMessage_Then_DapperMethodInvoked()
    {
        var id = new Identity(12);
        await _dataAccess.CancelPendingQueueMessage(new("QueueName"), id);
        _dapperMethods.Verify(d => d.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()), Times.Once());
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "INSERT INTO [PBus].[QueueName_Failed]");
        AssertTable(statement, "DELETE FROM [PBus].[QueueName_Pending]");
        Assert.IsNotNull(parameters);
        Assert.AreEqual(id, parameters.Get<Identity>("@Id"));
    }

    [TestMethod]
    public async Task When_CancelPendingSusbcribedMessage_Then_DapperMethodInvoked()
    {
        var id = new Identity(12);
        await _dataAccess.CancelPendingSubscribedMessage(id);
        _dapperMethods.Verify(d => d.Execute(It.IsAny<string>(), It.IsAny<DynamicParameters?>()), Times.Once());
        (var statement, var parameters) = GetArguments();
        AssertTable(statement, "INSERT INTO [PBus].[Subscribed_Failed]");
        AssertTable(statement, "DELETE FROM [PBus].[Subscribed_Pending]");
        Assert.IsNotNull(parameters);
        Assert.AreEqual(id, parameters.Get<Identity>("@Id"));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_CancelPendingQueueuMessage_Then_Throws()
    {
        Given_DapperMethodThrows();
        var id = new Identity(12);
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.CancelPendingQueueMessage(new("QueueName"), id));
    }

    [TestMethod]
    public async Task Given_DapperMethodThrows_When_CancelPendingSubscribedMessage_Then_Throws()
    {
        Given_DapperMethodThrows();
        var id = new Identity(12);
        await Assert.ThrowsAsync<TestException>(() =>
            _dataAccess.CancelPendingSubscribedMessage(id));
    }
}

