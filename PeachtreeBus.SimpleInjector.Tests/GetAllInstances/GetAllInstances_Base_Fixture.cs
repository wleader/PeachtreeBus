using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector.Tests.GetAllInstances;

public abstract class GetAllInstances_Base_Fixture<TInterface>
    where TInterface : class
{
    protected IWrappedScope Given_ContainerWithRegistrations(Action<Container>? addRegistrations = null)
    {
        var container = new Container();
        container.Options.AllowOverridingRegistrations = true;
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        container.Register(typeof(IWrappedScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
        container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);

        addRegistrations?.Invoke(container);

        container.Verify();

        var factory = container.GetInstance<IWrappedScopeFactory>();
        return factory.Create();
    }

    protected void AddRegistrations(Container container, IEnumerable<Type> concreteTypes)
    {
        container.Collection.Register(typeof(TInterface), concreteTypes, Lifestyle.Scoped);
        foreach (var t in concreteTypes)
        {
            Assert.IsTrue(typeof(TInterface).IsAssignableFrom(t));
            container.Register(t, t, Lifestyle.Scoped);
        }
    }

    protected abstract IEnumerable<Type> GetTypesToRegister();

    [TestMethod]
    public void Given_NoTypesRegistered_When_GetAllInstances_Then_Empty()
    {
        using var scope = Given_ContainerWithRegistrations(c => AddRegistrations(c, []));
        var handlers = scope.GetAllInstances<TInterface>();
        Assert.IsNotNull(handlers);
        Assert.IsFalse(handlers.Any());
    }

    [TestMethod]
    public void GivenTypesRegisterd_When_GetAllInstances_Then_NotEmpty()
    {
        using var scope = Given_ContainerWithRegistrations(c => AddRegistrations(c, GetTypesToRegister()));
        var handlers = scope.GetAllInstances<TInterface>();
        Assert.IsNotNull(handlers);
        Assert.AreEqual(2, handlers.Count());
    }
}
