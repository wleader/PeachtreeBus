﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscribedPendingTracker : ITracker;

public class CleanSubscribedPendingTracker(
    ISystemClock clock,
    IBusConfiguration config)
    : IntervalRunTracker(clock)//, config.SubscriptionConfiguration?.CleanInterval)
    , ICleanSubscribedPendingTracker
{
    private readonly IBusConfiguration _config = config;

    public override TimeSpan? Interval =>
        _config.SubscriptionConfiguration?.CleanInterval;
}

public interface ICleanSubscribedPendingTask : IBaseTask;

public class CleanSubscribedPendingTask(
    IBusConfiguration configuration,
    IBusDataAccess dataAccess)
    : ICleanSubscribedPendingTask
{
    private readonly IBusConfiguration _busConfiguration = configuration;
    private readonly IBusDataAccess _dataAccess = dataAccess;

    public async Task<bool> RunOne()
    {
        var config = _busConfiguration.SubscriptionConfiguration;
        if (config is null) return false;
        var rows = await _dataAccess.ExpireSubscriptionMessages(config.CleanMaxRows);
        return rows > 0;
    }
}

public interface ICleanSubscribedPendingRunner : IRunner;

public class CleanSubscribedPendingRunner(
    IBusDataAccess dataAccess,
    ILogger<CleanSubscribedPendingRunner> log,
    ICleanSubscribedPendingTask task)
    : Runner<ICleanSubscribedPendingTask>(dataAccess, log, task)
    , ICleanSubscribedPendingRunner;

public interface ICleanSubscribedPendingStarter : IStarter;

public class CleanSubscribedPendingStarter(
    ILogger<CleanSubscribedPendingStarter> log,
    IScopeFactory scopeFactory,
    ICurrentTasks tasks,
    ICleanSubscribedPendingTracker tracker,
    IScheduledTaskCounter counter,
    IAlwaysOneEstimator estimator,
    IBusDataAccess dataAccess)
    : Starter<ICleanSubscribedPendingRunner>(log, scopeFactory, tasks, tracker, counter, estimator, dataAccess)
    , ICleanSubscribedPendingStarter;