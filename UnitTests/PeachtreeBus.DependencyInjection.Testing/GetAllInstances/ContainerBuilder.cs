using System;
using System.Collections.Generic;

namespace PeachtreeBus.DependencyInjection.Testing.GetAllInstances;

public abstract class ContainerBuilder<TContainer>
{
    public abstract IWrappedScope CreateScope(Action<TContainer>? addRegistrations = null);
    public abstract void AddRegistrations<TInterface>(TContainer container, IEnumerable<Type> concreteTypes);
}
