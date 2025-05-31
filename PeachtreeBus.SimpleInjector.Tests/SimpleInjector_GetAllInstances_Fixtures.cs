using PeachtreeBus.DependencyInjection.Tests.GetAllInstances;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.SimpleInjector.Tests;

public class SimpleInjector_ContainerBuilder
    : ContainerBuilder<Container>
{
    public override IWrappedScope CreateScope(Action<Container>? addRegistrations = null)
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

    public override void AddRegistrations<TInterface>(Container container, IEnumerable<Type> concreteTypes)
    {
        container.Collection.Register(typeof(TInterface), concreteTypes, Lifestyle.Scoped);
        foreach (var t in concreteTypes)
        {
            Assert.IsTrue(typeof(TInterface).IsAssignableFrom(t));
            container.Register(t, t, Lifestyle.Scoped);
        }
    }
}


[TestClass]
public class SimpleInjector_GetAllInstances_QueueHandlers_Fixture()
    : GetAllInstances_QueueHandlers_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetAllInstances_SubscribedHandlers_Fixture()
     : GetAllInstances_SubscribedHandlers_Fixture<Container>(new SimpleInjector_ContainerBuilder());