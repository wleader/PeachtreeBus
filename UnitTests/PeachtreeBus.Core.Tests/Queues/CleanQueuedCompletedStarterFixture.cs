﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedCompletedStarterFixture : StarterFixtureBase<
    CleanQueuedCompletedStarter,
    ICleanQueuedCompletedRunner,
    ICleanQueuedCompletedTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    public override CleanQueuedCompletedStarter CreateStarter()
    {
        return new(
            _log.Object,
            _scopeFactory.Object,
            _tasks.Object,
            _tracker.Object,
            _taskCounter.Object,
            _estimator.Object,
            _dataAccess.Object);
    }
}
