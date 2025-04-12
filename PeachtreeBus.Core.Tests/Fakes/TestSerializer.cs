using PeachtreeBus.Serialization;

namespace PeachtreeBus.Core.Tests.Fakes;

public static class TestSerializer
{
    public static ISerializer Instance { get; } = new DefaultSerializer();
}
