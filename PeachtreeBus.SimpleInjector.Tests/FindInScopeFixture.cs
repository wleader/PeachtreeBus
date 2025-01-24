using Moq;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class FindInScopeFixture
{
    [TestMethod]
    public void FailedQueueMessageHandlerFactory_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        var instance = new Mock<IHandleFailedQueueMessages>();
        scope.Setup(s => s.GetInstance<IHandleFailedQueueMessages>())
            .Returns(instance.Object);
        var subject = new FailedQueueMessageHandlerFactory(scope.Object);
        var actual = subject.GetHandler();
        Assert.AreSame(instance.Object, actual);
    }

    [TestMethod]
    public void FailedSubscribedMessageHandlerFactory_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        var instance = new Mock<IHandleFailedSubscribedMessages>();
        scope.Setup(s => s.GetInstance<IHandleFailedSubscribedMessages>())
            .Returns(instance.Object);
        var subject = new FailedSubscribedMessageHandlerFactory(scope.Object);
        var actual = subject.GetHandler();
        Assert.AreSame(instance.Object, actual);
    }

    [TestMethod]
    public void FindSubscribedHandlers_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        var instance = new List<IHandleSubscribedMessage<TestSubscribedMessage>>();
        scope.Setup(s => s.GetAllInstances<IHandleSubscribedMessage<TestSubscribedMessage>>())
            .Returns(instance);
        var subject = new FindSubscribedHandlers(scope.Object);
        var actual = subject.FindHandlers<TestSubscribedMessage>();
        Assert.AreSame(instance, actual);
    }

    [TestMethod]
    public void FindQueueHandlers_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        var instance = new List<IHandleQueueMessage<TestQueueMessage>>();
        scope.Setup(s => s.GetAllInstances<IHandleQueueMessage<TestQueueMessage>>())
            .Returns(instance);
        var subject = new FindQueueHandlers(scope.Object);
        var actual = subject.FindHandlers<TestQueueMessage>();
        Assert.AreSame(instance, actual);
    }

    [TestMethod]
    public void FindSubscribedPipelineSteps_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        List<ISubscribedPipelineStep> instance =
            [new Mock<ISubscribedPipelineStep>().Object];
        scope.Setup(s => s.GetAllInstances<ISubscribedPipelineStep>())
            .Returns(instance);
        var subject = new FindSubscribedPipelineSteps(scope.Object);
        var actual = subject.FindSteps().ToList();
        CollectionAssert.AreEqual(instance, actual);
    }

    [TestMethod]
    public void FindQueuePipelineSteps_GetsFromScope()
    {
        var scope = new Mock<IWrappedScope>();
        List<IQueuePipelineStep> instance =
            [new Mock<IQueuePipelineStep>().Object];
        scope.Setup(s => s.GetAllInstances<IQueuePipelineStep>())
            .Returns(instance);
        var subject = new FindQueuedPipelineSteps(scope.Object);
        var actual = subject.FindSteps().ToList();
        CollectionAssert.AreEqual(instance, actual);
    }
}
