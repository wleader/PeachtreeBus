using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorScopeFixture
{
    private Container _container = default!;
    private Scope _scope = default!;

    private SimpleInjectorScope _subject = default!;
    private readonly Mock<IQueuePipelineStep> _queueInstance = new();
    private readonly List<Type> _subscribedTypes = [typeof(TestSubscribedPipelineStep)];

    [TestInitialize]
    public void Initialize()
    {
        _container = new Container();
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        _container.RegisterInstance(_queueInstance.Object);
        _container.Collection.Register(typeof(ISubscribedPipelineStep), _subscribedTypes, Lifestyle.Transient);
        _container.Register(typeof(TestSubscribedPipelineStep), typeof(TestSubscribedPipelineStep), Lifestyle.Transient);

        _scope = AsyncScopedLifestyle.BeginScope(_container);

        _subject = new()
        {
            Scope = _scope
        };
    }

    [TestMethod]
    public void Given_ScopeNull_When_GetAllInstances_Then_Throws()
    {
        _subject.Scope = null;
        Assert.ThrowsException<InvalidOperationException>(_subject.GetAllInstances<IQueuePipelineStep>);
    }

    [TestMethod]
    public void Given_ScopeNull_When_GetInstanceGeneric_Then_Throws()
    {
        _subject.Scope = null;
        Assert.ThrowsException<InvalidOperationException>(_subject.GetInstance<IQueuePipelineStep>);
    }


    [TestMethod]
    public void Given_ScopeNull_When_GetInstance_Then_Throws()
    {
        _subject.Scope = null;
        Assert.ThrowsException<InvalidOperationException>(() => _subject.GetInstance(typeof(IQueuePipelineStep)));
    }

    [TestMethod]
    public void Given_Scope_When_GetAllInstances_Then_Result()
    {
        Assert.IsNotNull(_subject.Scope);
        var actual = _subject.GetAllInstances<ISubscribedPipelineStep>();
        Assert.IsNotNull(actual);
        var instances = actual.ToList();
        Assert.AreEqual(1, instances.Count);
        Assert.AreEqual(typeof(TestSubscribedPipelineStep), instances[0].GetType());
    }


    [TestMethod]
    public void Given_Scope_When_GetInstanceGeneric_Then_Result()
    {
        Assert.IsNotNull(_subject.Scope);
        var actual = _subject.GetInstance<IQueuePipelineStep>();
        Assert.IsNotNull(actual);
    }

    [TestMethod]
    public void Given_Scope_When_GetInstance_Then_Result()
    {
        Assert.IsNotNull(_subject.Scope);
        var actual = _subject.GetInstance(typeof(IQueuePipelineStep));
        Assert.IsNotNull(actual);
    }
}
