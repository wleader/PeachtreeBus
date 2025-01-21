using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Errors;

namespace PeachtreeBus.Tests.Errors;

[TestClass]
public class DefaultFailedQueueMessageHandlerFixture
{
    [TestMethod]
    public void When_Handle_Then_Nothing()
    {
        var handler = new DefaultFailedQueueMessageHandler();
        var result = handler.Handle(null!, null!, null!);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCompleted);
    }
}
