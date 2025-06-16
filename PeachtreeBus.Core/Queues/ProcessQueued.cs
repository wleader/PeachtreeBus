using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface IProcessQueuedTask : IBaseTask;

public class ProcessQueuedTask(
    IBusConfiguration configuration,
    ISystemClock clock,
    ILogger<ProcessQueuedTask> log,
    IQueueReader queueReader,
    IMeters meters,
    IBusDataAccess dataAccess,
    IQueuePipelineInvoker pipelineInvoker)
    : IProcessQueuedTask
{
    private readonly IBusConfiguration _configuration = configuration;
    private readonly ISystemClock _clock = clock;
    private readonly ILogger<ProcessQueuedTask> _log = log;
    private readonly IQueueReader _queueReader = queueReader;
    private readonly IMeters _meters = meters;
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IQueuePipelineInvoker _pipelineInvoker = pipelineInvoker;

    private const string SavepointName = "BeforeMessageHandler";

    public async Task<bool> RunOne()
    {
        var queueConfig = _configuration.QueueConfiguration;

        if (queueConfig is null) return false;

        // make a note of when we started, so that we can back-date the activity
        // if we found a message to process.
        var started = _clock.UtcNow;

        // get a message.
        var context = await _queueReader.GetNext(queueConfig.QueueName);

        // if no messages, roll back the transaction, and end the task runner.
        if (context == null)
            return false;


        // we got a message, generate an activity.
        using var activity = new ReceiveActivity(context, started);

        // we found a message to process.
        _log.ProcessingMessage(context.MessageId, context.MessageClass);

        try
        {
            _meters.StartMessage();

            // creat a save point. If anything goes wrong we can roll back to here,
            // increment the retry count and try again later.
            _dataAccess.CreateSavepoint(SavepointName);

            await _pipelineInvoker.Invoke(context);

            if (context.SagaBlocked)
            {
                // the saga is blocked. delay the message and try again later.
                _log.SagaBlocked(context.CurrentHandler!, context.SagaKey);
                _dataAccess.RollbackToSavepoint(SavepointName);
                await _queueReader.DelayMessage(context, 250);
                _meters.SagaBlocked();
                return true;
            }

            // if nothing threw an exception, we can mark the message as processed.
            await _queueReader.Complete(context);
        }
        catch (Exception ex)
        {
            // there was an exception, Rollback to the save point to undo
            // any db changes done by the handlers.
            _log.HandlerException(context.CurrentHandler!, context.MessageId, context.MessageClass, ex);
            _dataAccess.RollbackToSavepoint(SavepointName);
            // increment the retry count, (or maybe even fail the message)
            await _queueReader.Fail(context, ex);

            activity.AddException(ex);
        }
        finally
        {
            _meters.FinishMessage();
        }
        return true;
    }
}

public interface IProcessQueuedRunner : IRunner;

public class ProcessQueuedRunner(
    IBusDataAccess dataAccess,
    ILogger<ProcessQueuedRunner> log,
    IProcessQueuedTask task)
    : Runner<IProcessQueuedTask>(dataAccess, log, task)
    , IProcessQueuedRunner;


public interface IProcessQueuedStarter : IStarter;

public class ProcessQueuedStarter(
    IWrappedScopeFactory scopeFactory,
    IAlwaysRunTracker tracker,
    IBusDataAccess dataAccess,
    IBusConfiguration busConfiguration,
    ITaskCounter counter)
    : Starter<IProcessQueuedRunner>(scopeFactory, tracker, counter)
    , IProcessQueuedStarter
{
    private readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly IBusConfiguration _busConfiguration = busConfiguration;

    protected override async Task<int> EstimateDemand()
    {
        var c = _busConfiguration.QueueConfiguration;
        return c is null
            ? 0
            : (int)await _dataAccess.EstimateQueuePending(c.QueueName);
    }
}
