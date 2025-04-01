using PeachtreeBus.Serialization;

namespace PeachtreeBus.Tests.Fakes;

public static class TestSerializer
{
    public static ISerializer Instance { get; } = new DefaultSerializer();
}
