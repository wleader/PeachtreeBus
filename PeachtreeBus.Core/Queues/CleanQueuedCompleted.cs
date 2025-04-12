using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface ICleanQueuedCompletedTracker : ITracker;

public class CleanQueuedCompletedTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)
    , ICleanQueuedCompletedTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.QueueConfiguration?.CleanInterval;
}

public interface ICleanQueuedCompletedTask : IBaseTask;

public class CleanQueuedCompletedTask(
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ISystemClock clock)
    : ICleanQueuedCompletedTask
{
    private readonly ISystemClock _clock = clock;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IBusConfiguration _config = configuration;

    public async Task<bool> RunOne()
    {
        var config = _config.QueueConfiguration;
        if (config is null) return false;
        var olderThan = _clock.UtcNow.Subtract(config.CleanCompleteAge);
        var rows = await _dataAccess.CleanQueueCompleted(config.QueueName, olderThan, config.CleanMaxRows);
        return rows != 0;
    }
}

public interface ICleanQueuedCompletedRunner : IRunner;

public class CleanQueuedCompletedRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanQueuedCompletedRunner> log,
    ICleanQueuedCompletedTask task)
    : Runner<ICleanQueuedCompletedTask>(dataAccess, log, task, "CleanQueuedCompleted")
    , ICleanQueuedCompletedRunner;


public interface ICleanQueuedCompletedStarter : IStarter;

public class CleanQueuedCompletedStarter(
    IWrappedScopeFactory scopeFactory,
    ICleanQueuedCompletedTracker tracker)
    : Starter<ICleanQueuedCompletedRunner>(scopeFactory, tracker)
    , ICleanQueuedCompletedStarter;