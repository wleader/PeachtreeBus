using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface ICleanQueuedFailedTracker : ITracker;

public class CleanQueuedFailedTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)
    , ICleanQueuedFailedTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.QueueConfiguration?.CleanInterval;
}

public interface ICleanQueuedFailedTask : IBaseTask;

public class CleanQueuedFailedTask(
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ISystemClock clock)
    : ICleanQueuedFailedTask
{
    private readonly ISystemClock _clock = clock;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IBusConfiguration _config = configuration;

    public async Task<bool> RunOne()
    {
        var config = _config.QueueConfiguration;
        if (config is null) return false;
        var olderThan = _clock.UtcNow.Subtract(config.CleanCompleteAge);
        var rows = await _dataAccess.CleanQueueFailed(config.QueueName, olderThan, config.CleanMaxRows);
        return rows != 0;
    }
}

public interface ICleanQueuedFailedRunner : IRunner;

public class CleanQueuedFailedRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanQueuedFailedRunner> log,
    ICleanQueuedFailedTask task)
    : Runner<ICleanQueuedFailedTask>(dataAccess, log, task)
    , ICleanQueuedFailedRunner;

public interface ICleanQueuedFailedStarter : IStarter;

public class CleanQueuedFailedStarter(
    ILogger<CleanQueuedFailedStarter> log,
    IScopeFactory scopeFactory,
    ICleanQueuedFailedTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<ICleanQueuedFailedRunner>(log, scopeFactory, tracker, counter, estimator, dataAccess)
    , ICleanQueuedFailedStarter;
