using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IUpdateSubscriptionsTracker : ITracker;

public class UpdateSubscriptionsTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)
    , IUpdateSubscriptionsTracker
{
    private readonly IBusConfiguration _config = config;

    // divide by two so that we update the subscriptions well
    // before they expire.
    public override TimeSpan? Interval =>
        _config.SubscriptionConfiguration is null
            ? null
            : _config.SubscriptionConfiguration.Lifespan / 2;
}

public interface IUpdateSubscriptionsTask : IBaseTask;

public class UpdateSubscriptionsTask(
    IBusDataAccess dataAccess,
    IBusConfiguration configuration,
    ISystemClock clock)
    : IUpdateSubscriptionsTask
{
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ISystemClock _clock = clock;
    private readonly IBusConfiguration _config = configuration;
    private bool _taskComplete = false;
    public async Task<bool> RunOne()
    {
        var config = _config.SubscriptionConfiguration;
        if (_taskComplete || config is null) return false;
        var until = _clock.UtcNow.Add(config.Lifespan);
        foreach (var topic in config.Topics)
        {
            await _dataAccess.Subscribe(
                config.SubscriberId,
                topic,
                until);
        }
        _taskComplete = true;
        return true;
    }
}

public interface IUpdateSubscriptionsRunner : IRunner;

public class UpdateSubscriptionsRunner(
    IBusDataAccess dataAccess,
    ILogger<UpdateSubscriptionsRunner> log,
    IUpdateSubscriptionsTask task)
    : Runner<IUpdateSubscriptionsTask>(dataAccess, log, task)
    , IUpdateSubscriptionsRunner;

public interface IUpdateSubscriptionsStarter : IStarter;

public class UpdateSubscriptionsStarter(
    ILogger<UpdateSubscriptionsStarter> log,
    IScopeFactory scopeFactory,
    ICurrentTasks tasks, 
    IUpdateSubscriptionsTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<IUpdateSubscriptionsRunner>(log, scopeFactory, tasks, tracker, counter, estimator, dataAccess)
    , IUpdateSubscriptionsStarter;