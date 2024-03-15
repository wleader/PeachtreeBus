using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
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
    public class QueueWork(
        ILogger<QueueWork> log,
        IPerfCounters counters,
        IQueueReader queueReader,
        IBusDataAccess dataAccess,
        IQueuePipelineInvoker pipelineInvoker) : IQueueWork
    {
        private readonly ILogger<QueueWork> _log = log;
        private readonly IPerfCounters _counters = counters;
        private readonly IQueueReader _queueReader = queueReader;
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IQueuePipelineInvoker _pipelineInvoker = pipelineInvoker;

        public string QueueName { get; set; } = string.Empty;

        private const string savepointName = "BeforeMessageHandler";

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
                                
                await _pipelineInvoker.Invoke(context);

                if (context.SagaBlocked)
                {
                    // the saga is blocked. delay the message and try again later.
                    _log.QueueWork_SagaBlocked(context.CurrentHandler!, context.SagaKey);
                    _dataAccess.RollbackToSavepoint(savepointName);
                    await _queueReader.DelayMessage(context, 250);
                    _counters.SagaBlocked();
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
                _log.QueueWork_HandlerException(context.CurrentHandler!, context.MessageData.MessageId, context.Headers.MessageClass, ex);
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
    }
}
