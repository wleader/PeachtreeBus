using PeachtreeBus.Data;
using PeachtreeBus.Serialization;

namespace PeachtreeBus.Core.Tests.Fakes;

public static class TestDapperTypesHandler
{
    public static DapperTypesHandler Instance { get; }
        = new(new DefaultSerializer());
}
