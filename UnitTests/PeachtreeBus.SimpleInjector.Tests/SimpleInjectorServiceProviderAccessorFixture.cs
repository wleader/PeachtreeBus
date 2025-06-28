using Moq;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorServiceProviderAccessorFixture
{
    private Container _container = default!;
    private Scope _scope = default!;

    private SimpleInjectorServiceProviderAccessor _subject = default!;
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

        _subject = new();
    }

    [TestMethod]
    public void Given_ScopeNull_When_GetAllInstances_Then_Throws()
    {
        Assert.ThrowsExactly<ServiceProviderAccessorException>(() => _subject.GetService<IQueuePipelineStep>());
    }

    [TestMethod]
    public void Given_ScopeNull_When_GetInstanceGeneric_Then_Throws()
    {
        Assert.ThrowsExactly<ServiceProviderAccessorException>(() => _subject.GetService<IQueuePipelineStep>());
    }

    [TestMethod]
    public void Given_Scope_When_GetServices_Then_Result()
    {
        _subject.Initialize(_scope, _scope);
        var actual = _subject.GetServices<ISubscribedPipelineStep>();
        Assert.IsNotNull(actual);
        var instances = actual.ToList();
        Assert.AreEqual(1, instances.Count);
        Assert.AreEqual(typeof(TestSubscribedPipelineStep), instances[0].GetType());
    }


    [TestMethod]
    public void Given_Scope_When_GetInstanceGeneric_Then_Result()
    {
        _subject.Initialize(_scope, _scope);
        var actual = _subject.GetService<IQueuePipelineStep>();
        Assert.IsNotNull(actual);
    }
}
