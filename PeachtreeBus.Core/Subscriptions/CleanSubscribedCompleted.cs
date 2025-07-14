using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscribedCompletedTracker : ITracker;

public class CleanSubscribedCompletedTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)
    , ICleanSubscribedCompletedTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.SubscriptionConfiguration?.CleanInterval;
}

public interface ICleanSubscribedCompletedTask : IBaseTask;

public class CleanSubscribedCompletedTask(
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ISystemClock clock)
    : ICleanSubscribedCompletedTask
{
    private readonly IBusConfiguration _busConfiguration = configuration;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ISystemClock _clock = clock;

    public async Task<bool> RunOne()
    {
        var config = _busConfiguration.SubscriptionConfiguration;
        if (config is null) return false;
        var olderThan = _clock.UtcNow.Subtract(config.CleanCompleteAge);
        var rows = await _dataAccess.CleanSubscribedCompleted(olderThan, config.CleanMaxRows);
        return rows > 0;
    }
}

public interface ICleanSubscribedCompletedRunner : IRunner;

public class CleanSubscribedCompletedRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanSubscribedCompletedRunner> log,
    ICleanSubscribedCompletedTask task)
    : Runner<ICleanSubscribedCompletedTask>(dataAccess, log, task)
    , ICleanSubscribedCompletedRunner;

public interface ICleanSubscribedCompletedStarter : IStarter;

public class CleanSubscribedCompletedStarter(
    ILogger<CleanSubscribedCompletedStarter> log,
    IScopeFactory scopeFactory,
    ICleanSubscribedCompletedTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<ICleanSubscribedCompletedRunner>(log, scopeFactory, tracker, counter, estimator, dataAccess)
    , ICleanSubscribedCompletedStarter;
