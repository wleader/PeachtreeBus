using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscribedFailedTracker : ITracker;

public class CleanSubscribedFailedTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)//, config.SubscriptionConfiguration?.CleanInterval)
    , ICleanSubscribedFailedTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.SubscriptionConfiguration?.CleanInterval;
}

public interface ICleanSubscribedFailedTask : IBaseTask;

public class CleanSubscribedFailedTask(
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ISystemClock clock)
    : ICleanSubscribedFailedTask
{
    private readonly IBusConfiguration _busConfiguration = configuration;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ISystemClock _clock = clock;

    public async Task<bool> RunOne()
    {
        var config = _busConfiguration.SubscriptionConfiguration;
        if (config is null) return false;
        var olderThan = _clock.UtcNow.Subtract(config.CleanFailedAge);
        var rows = await _dataAccess.CleanSubscribedFailed(olderThan, config.CleanMaxRows);
        return rows > 0;
    }
}

public interface ICleanSubscribedFailedRunner : IRunner;

public class CleanSubscribedFailedRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanSubscribedFailedRunner> log,
    ICleanSubscribedFailedTask task)
    : Runner<ICleanSubscribedFailedTask>(dataAccess, log, task)
    , ICleanSubscribedFailedRunner;

public interface ICleanSubscribedFailedStarter : IStarter;

public class CleanSubscribedFailedStarter(
    ILogger<CleanSubscribedFailedStarter> log,
    IScopeFactory scopeFactory,
    ICleanSubscribedFailedTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<ICleanSubscribedFailedRunner>(log, scopeFactory, tracker, counter, estimator, dataAccess)
    , ICleanSubscribedFailedStarter;