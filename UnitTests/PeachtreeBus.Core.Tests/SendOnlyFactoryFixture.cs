using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Testing;
using System;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class SendOnlyFactoryFixture
{
    private SendOnlyFactory _factory = default!;
    private readonly SqlConnection _connection = SqlServerTesting.CreateConnection();
    private readonly SqlTransaction? _transaction = SqlServerTesting.CreateTransaction();
    private readonly FakeServiceProviderAccessor _accessor = new(null);

    private readonly FakeServiceProvider _provider = new();

    [TestInitialize]
    public void Initialize()
    {
        _accessor.Reset();

        _provider.Reset();
        _provider.AddMock(_accessor.Mock);
        _provider.AddMock<IShareObjectsBetweenScopes>();
        _provider.AddMock<ISharedDatabase>();
        _provider.AddMock<IQueueWriter>();
        _provider.AddMock<ISubscribedPublisher>();

        _factory = new();
    }

    private void Given_Configured(bool configured)
    {
        _accessor.Reset(configured ? _provider.Object : null);
    }


    private IQueueWriter When_CreateQueueWriter() =>
        _factory.CreateQueueWriter(_provider.Object, _connection, _transaction);

    private ISubscribedPublisher When_CreateSubscribedPublisher() =>
        _factory.CreateSubscribedPublisher(_provider.Object, _connection, _transaction);

    private void Then_UseExistingIsNotInvoked() =>
        _accessor.Mock.Verify(x => x.UseExisting(It.IsAny<IServiceProvider>()), Times.Never);

    private void Then_ObjectsAreSetupOnce()
    {
        _accessor.Mock.Verify(x => x.UseExisting(_provider.Object), Times.Once());
        var sharedDb = _provider.GetMock<ISharedDatabase>();
        sharedDb.Verify(x => x.SetExternallyManagedConnection(_connection, _transaction), Times.Once);
        var shareObject = _provider.GetMock<IShareObjectsBetweenScopes>();
        shareObject.VerifySet(x => x.SharedDatabase = sharedDb.Object, Times.Once);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "Accessor Configured")]
    [DataRow(false, DisplayName = "Accessor Not Configured")]
    public void Given_Configure_When_CreateQueueWriter_Then_QueueWriterCreated(bool configure)
    {
        Given_Configured(configure);
        Assert.AreSame(
            _provider.GetRegistered<IQueueWriter>(),
            When_CreateQueueWriter());
    }

    [TestMethod]
    [DataRow(true, DisplayName = "Accessor Configured")]
    [DataRow(false, DisplayName = "Accessor Not Configured")]
    public void Given_Configure_When_CreateSubscribedPublisher_Then_SubscribedPublisherCreated(bool configure)
    {
        Given_Configured(configure);
        Assert.AreSame(
            _provider.GetRegistered<ISubscribedPublisher>(),
            When_CreateSubscribedPublisher());
    }


    [TestMethod]
    public void Given_NotConfigured_When_CreateQueueWriterTwice_Then_ObjectsAreSetupOnce()
    {
        Given_Configured(false);
        When_CreateQueueWriter();
        When_CreateQueueWriter();
        Then_ObjectsAreSetupOnce();
    }

    [TestMethod]
    public void Given_NotConfigured_When_CreateSubscribedPublisherTwice_Then_ObjectsAreSetupOnce()
    {
        Given_Configured(false);
        When_CreateSubscribedPublisher();
        When_CreateSubscribedPublisher();
        Then_ObjectsAreSetupOnce();
    }

    [TestMethod]
    public void Given_NotConfigured_When_CreateBoth_Then_ObjectsAreSetupOnce()
    {
        Given_Configured(false);
        When_CreateQueueWriter();
        When_CreateSubscribedPublisher();
        Then_ObjectsAreSetupOnce();
    }

    [TestMethod]
    public void Given_Configured_When_CreateQeueuWriter_Then_UseExistingNotInvoked()
    {
        Given_Configured(true);
        When_CreateQueueWriter();
        Then_UseExistingIsNotInvoked();
    }

    [TestMethod]
    public void Given_Configured_When_CreateSubscribedPulisher_Then_UseExistingNotInvoked()
    {
        Given_Configured(true);
        When_CreateSubscribedPublisher();
        Then_UseExistingIsNotInvoked();
    }
}
