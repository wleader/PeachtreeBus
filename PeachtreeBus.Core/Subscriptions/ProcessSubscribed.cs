using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IProcessSubscribedTask : IBaseTask;

public class ProcessSubscribedTask(
    ILogger<ProcessSubscribedTask> log,
    ISystemClock clock,
    ISubscribedReader reader,
    IBusConfiguration configuration,
    IMeters meters,
    IBusDataAccess dataAccess,
    ISubscribedPipelineInvoker pipelineInvoker)
    : IProcessSubscribedTask
{
    private readonly IBusConfiguration _configuration = configuration;
    private readonly ILogger<ProcessSubscribedTask> _log = log;
    private readonly ISystemClock _clock = clock;
    private readonly ISubscribedReader _reader = reader;
    private readonly IMeters _meters = meters;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ISubscribedPipelineInvoker _pipelineInvoker = pipelineInvoker;

    public async Task<bool> RunOne()
    {
        // if this throws, its because the tracker
        // retured true to ShouldStart when it should't have.
        var sConfig = UnreachableException.ThrowIfNull(_configuration.SubscriptionConfiguration);

        const string savepointName = "BeforeSubscriptionHandler";

        var started = _clock.UtcNow;

        // get a message.
        var context = await _reader.GetNext(sConfig.SubscriberId);

        // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
        if (context == null)
            return false;

        using var activity = new ReceiveActivity(context, started);

        // we found a message to process.
        _log.ProcessingMessage(
            context.Data.MessageId,
            context.SubscriberId);

        try
        {
            _meters.StartMessage();

            // creat a save point. If anything goes wrong we can roll back to here,
            // increment the retry count and try again later.
            _dataAccess.CreateSavepoint(savepointName);

            await _pipelineInvoker.Invoke(context);

            // if nothing threw an exception, we can mark the message as processed.
            await _reader.Complete(context);
        }
        catch (Exception ex)
        {
            // there was an exception, Rollback to the save point to undo
            // any db changes done by the handlers.
            _log.MessageHandlerException(
                context.Data.MessageId,
                context.SubscriberId,
                ex);
            _dataAccess.RollbackToSavepoint(savepointName);
            // increment the retry count, (or maybe even fail the message)
            await _reader.Fail(context, ex);
            activity.AddException(ex);
        }
        finally
        {
            _meters.FinishMessage();
        }
        return true;
    }
}

public interface IProcessSubscribedRunner : IRunner;

public class ProcessSubscribedRunner(
    IBusDataAccess dataAccess,
    ILogger<ProcessSubscribedRunner> log,
    IProcessSubscribedTask task)
    : Runner<IProcessSubscribedTask>(dataAccess, log, task)
    , IProcessSubscribedRunner;

public interface IProcessSubscribedStarter : IStarter;

public class ProcessSubscribedStarter(
    IScopeFactory scopeFactory,
    IAlwaysRunTracker tracker,
    IBusDataAccess dataAccess,
    IBusConfiguration busConfiguration,
    ITaskCounter counter)
    : Starter<IProcessSubscribedRunner>(scopeFactory, tracker, counter)
    , IProcessSubscribedStarter
{
    private readonly IBusConfiguration _busConfiguration = busConfiguration;
    private readonly IBusDataAccess _dataAccess = dataAccess;

    protected override async Task<int> EstimateDemand()
    {
        var c = _busConfiguration.SubscriptionConfiguration;
        return c is null
            ? 0
            : (int)await _dataAccess.EstimateSubscribedPending(c.SubscriberId);
    }
}
public interface IProcessSubscribedEstimator : IEstimator;

public class ProcessSubscribedEstimator(
    IBusDataAccess dataAccess,
    IBusConfiguration busConfiguration)
    : IProcessSubscribedEstimator
{
    public async Task<int> EstimateDemand()
    {
        var c = busConfiguration.SubscriptionConfiguration;
        return c is null
            ? 0
            : (int)await dataAccess.EstimateSubscribedPending(c.SubscriberId);
    }
}