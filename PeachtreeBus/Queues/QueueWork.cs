using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Sagas;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Reads one QueueMessage, and dispatches it through known message handlers.
    /// </summary>
    public interface IQueueWork : IUnitOfWork
    {
        public string QueueName { get; set; }
    }

    /// <inheritdoc/>>
    public class QueueWork : IQueueWork
    {
        private readonly ILogger<QueueWork> _log;
        private readonly IPerfCounters _counters;
        private readonly IFindQueueHandlers _findHandlers;
        private readonly IFindQueuePipelineSteps _findPipelineSteps;
        private readonly IQueueReader _queueReader;
        private readonly IBusDataAccess _dataAccess;
        private readonly ISagaMessageMapManager _sagaMessageMapManager;

        public QueueWork(
            ILogger<QueueWork> log,
            IPerfCounters counters,
            IFindQueueHandlers findHandlers,
            IFindQueuePipelineSteps findPipelineSteps,
            IQueueReader queueReader,
            IBusDataAccess dataAccess,
            ISagaMessageMapManager sagaMessageMapManager)
        {
            _log = log;
            _counters = counters;
            _findHandlers = findHandlers;
            _findPipelineSteps = findPipelineSteps;
            _queueReader = queueReader;
            _dataAccess = dataAccess;
            _sagaMessageMapManager = sagaMessageMapManager;
        }

        public string QueueName { get; set; }

        private const string savepointName = "BeforeMessageHandler";

        private string _currentHandlerTypeName = null;
        private bool _sagaBlocked = false;

        /// <summary>
        /// Actually does the work of processing a single message.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<bool> DoWork()
        {
            // get a message.
            var context = await _queueReader.GetNext(QueueName);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (context == null)
            {
                return false;
            }

            // we found a message to process.
            _log.QueueWork_ProcessingMessage(context.MessageData.MessageId, context.Headers.MessageClass);

            var started = DateTime.UtcNow;

            try
            {
                _counters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _dataAccess.CreateSavepoint(savepointName);

                _sagaBlocked = false;

                await InvokePipeline(context);

                if (_sagaBlocked)
                {
                    return true;
                }

                // if nothing threw an exception, we can mark the message as processed.
                await _queueReader.Complete(context);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.QueueWork_HandlerException(_currentHandlerTypeName, context.MessageData.MessageId, context.Headers.MessageClass, ex);
                _dataAccess.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _queueReader.Fail(context, ex);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            finally
            {
                _counters.FinishMessage(started);
            }
        }

        private async Task InvokePipeline(QueueContext context)
        {
            var steps = _findPipelineSteps.FindSteps().OrderBy(s => s.Priority);

            var pipeline = new Pipeline<QueueContext>();
            foreach (var step in steps)
            {
                pipeline.Add(step);
            }

            var handlersStep = new QueueHandlersPipelineStep(QueueName, _findHandlers, _log,
                _sagaMessageMapManager, _queueReader, _counters, _dataAccess, savepointName);
            pipeline.Add(handlersStep);

            await pipeline.Invoke(context);

            _sagaBlocked = handlersStep.SagaBlocked;
            _currentHandlerTypeName = handlersStep.CurrentHandlerTypeName;
        }

    }
}
