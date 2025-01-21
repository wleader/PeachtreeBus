﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Errors;

namespace PeachtreeBus.Tests.Errors;

[TestClass]
public class DefaultFailedSubscribedMessageHandlerFixture
{
    [TestMethod]
    public void When_Handle_Then_Nothing()
    {
        var handler = new DefaultFailedSubscribedMessageHandler();
        var result = handler.Handle(null!, null!, null!);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCompleted);
    }
}