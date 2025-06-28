using PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.SimpleInjector.Tests;

public class SimpleInjector_ContainerBuilder
    : ContainerBuilder<Container>
{
    public override IServiceProviderAccessor CreateScope(Action<Container>? addRegistrations = null)
    {
        var container = new Container();
        container.Options.AllowOverridingRegistrations = true;
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        container.Register(typeof(IScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
        container.Register(typeof(IServiceProviderAccessor), typeof(SimpleInjectorServiceProviderAccessor), Lifestyle.Scoped);

        addRegistrations?.Invoke(container);

        container.Verify();

        var factory = container.GetInstance<IScopeFactory>();
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
public class SimpleInjector_GetEnumerableOfIHandleQueueMessage_Fixture()
    : GetEnumerableOfIHandleQueueMessage_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetEnumerableOfIHandleSubscribedMessage_Fixture()
     : GetEnumerableOfIHandleSubscribedMessage_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetEnumerableOfIQueuePipelineStep_Fixture()
     : GetEnumerableOfIQueuePipelineStep_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetEnumerableOfISendPipelineStep_Fixture()
     : GetEnumerableOfISendPipelineStep_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetEnumerbaleOfIPublishPipelineStep_Fixture()
     : GetEnumerbaleOfIPublishPipelineStep_Fixture<Container>(new SimpleInjector_ContainerBuilder());

[TestClass]
public class SimpleInjector_GetEnumerableOfISubscribedPipelineStep_Fixture()
     : GetEnumerableOfISubscribedPipelineStep_Fixture<Container>(new SimpleInjector_ContainerBuilder());
