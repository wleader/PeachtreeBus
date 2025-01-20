﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Interfaces;

namespace PeachtreeBus.Tests.Cleaners;

[TestClass]
public class QueueCleanupWorkFixture
{
    [TestMethod]
    public void When_New_Then_Create()
    {
        var config = new Mock<IQueueCleanerConfiguration>();
        var clock = new Mock<ISystemClock>();
        var cleaner = new Mock<IQueueCleaner>();
        var w = new QueueCleanupWork(config.Object, clock.Object, cleaner.Object);
        Assert.IsNotNull(w);
    }
}
