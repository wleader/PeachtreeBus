using PeachtreeBus.ClassNames;
using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class CachedClassNameServiceDecorationFixture : SimpleInjectorExtensionFixtureBase
{
    [TestMethod]
    public void When_GetClassNameService_Then_ResultIsCachedClassNameService()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();

        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();
        var actual = scope.GetInstance<IClassNameService>();
        var cached = actual as CachedClassNameService;
        Assert.IsNotNull(cached);
    }
}
