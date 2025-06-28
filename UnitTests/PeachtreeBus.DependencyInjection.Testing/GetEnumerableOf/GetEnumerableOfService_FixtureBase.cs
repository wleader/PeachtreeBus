using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;

public abstract class GetEnumerableOfService_FixtureBase<TInterface, TContainer>(
    ContainerBuilder<TContainer> builder)
    where TInterface : class
{
    protected abstract IEnumerable<Type> GetTypesToRegister();

    [TestMethod]
    public void Given_NoTypesRegistered_When_GetEnumerableOfService_Then_Empty()
    {
        using var scope = builder.CreateScope(c => builder.AddRegistrations<TInterface>(c, []));
        var handlers = scope.GetRequiredService<IEnumerable<TInterface>>();
        Assert.IsNotNull(handlers);
        Assert.IsFalse(handlers.Any());
    }

    [TestMethod]
    public void GivenTypesRegisterd_When_GetEnumerableOFService_Then_NotEmpty()
    {
        using var scope = builder.CreateScope(c => builder.AddRegistrations<TInterface>(c, GetTypesToRegister()));
        var handlers = scope.GetRequiredService<IEnumerable<TInterface>>();
        Assert.IsNotNull(handlers);
        Assert.AreEqual(2, handlers.Count());
    }
}
