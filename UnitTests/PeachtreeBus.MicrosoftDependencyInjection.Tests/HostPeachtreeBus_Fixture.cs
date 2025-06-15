using Microsoft.Extensions.Hosting;
using System.Linq;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class HostPeachtreeBus_Fixture
{
    [TestMethod]
    public void When_HostPeachtreeBus_Then_HostedServiceIsRegistered()
    {
        var builder = new HostApplicationBuilder();
        builder.HostPeachtreeBus();
        Assert.IsNotNull(builder.Services.FirstOrDefault(s => s.ImplementationType == typeof(PeachtreeBusHostedService)));
    }
}
