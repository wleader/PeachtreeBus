using Microsoft.Extensions.Logging;
using Moq;

namespace PeachtreeBus.Core.Tests.Fakes;

public static class FakeLog
{
    public static ILogger<T> Create<T>() => new Mock<ILogger<T>>().Object;
}
