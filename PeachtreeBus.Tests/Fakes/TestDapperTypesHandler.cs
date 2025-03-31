using PeachtreeBus.Data;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.Tests.Fakes;

public static class TestDapperTypesHandler
{
    public static DapperTypesHandler Instance { get; }
        = new(new DefaultSerializer());
}
