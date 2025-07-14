using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscriptionsTracker : ITracker;

public class CleanSubscriptionsTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)
    , ICleanSubscriptionsTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.SubscriptionConfiguration?.CleanInterval;
}

public interface ICleanSubscriptionsTask : IBaseTask;

public class CleanSubscriptionsTask(
    IBusConfiguration busConfig,
    IBusDataAccess dataAccess)
    : ICleanSubscriptionsTask
{
    private readonly IBusConfiguration _busConfig = busConfig;
    private readonly IBusDataAccess _dataAccess = dataAccess;

    public async Task<bool> RunOne()
    {
        var config = _busConfig.SubscriptionConfiguration;
        if (config is null) return false;
        var rows = await _dataAccess.ExpireSubscriptions(config.CleanMaxRows);
        return rows != 0;
    }
}

public interface ICleanSubscriptionsRunner : IRunner;

public class CleanSubscriptionsRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanSubscriptionsRunner> log,
    ICleanSubscriptionsTask task)
    : Runner<ICleanSubscriptionsTask>(dataAccess, log, task)
    , ICleanSubscriptionsRunner;

public interface ICleanSubscriptionsStarter : IStarter;

public class CleanSubscriptionsStarter(
    ILogger<CleanSubscriptionsStarter> log,
    IScopeFactory scopeFactory,
    ICurrentTasks tasks,
    ICleanSubscriptionsTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<ICleanSubscriptionsRunner>(log, scopeFactory, tasks, tracker, counter, estimator, dataAccess)
    , ICleanSubscriptionsStarter;