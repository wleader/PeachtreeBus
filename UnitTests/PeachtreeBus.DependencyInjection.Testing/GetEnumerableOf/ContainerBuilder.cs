using System;
using System.Collections.Generic;

namespace PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;

public abstract class ContainerBuilder<TContainer>
{
    public abstract IServiceProviderAccessor CreateScope(Action<TContainer>? addRegistrations = null);
    public abstract void AddRegistrations<TInterface>(TContainer container, IEnumerable<Type> concreteTypes);
}
