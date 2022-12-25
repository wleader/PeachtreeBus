using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Describes the unit of work that reads one subscribed messsage and processes it.
    /// </summary>
    public interface ISubscribedWork : IUnitOfWork
    {
        Guid SubscriberId { get; set; }
    }

    /// <summary>
    /// A unit of work that reads one subscribed message and processes it.
    /// </summary>
    public class SubscribedWork : ISubscribedWork
    {
        private readonly ISubscribedReader _reader;
        private readonly IPerfCounters _counters;
        private readonly ILogger<SubscribedWork> _log;
        private readonly IBusDataAccess _dataAccess;
        private readonly IFindSubscribedHandlers _findSubscriptionHandlers;
        private readonly IFindSubscribedPipelineSteps _findPipelineSteps;

        public SubscribedWork(
            ISubscribedReader reader,
            IPerfCounters counters,
            ILogger<SubscribedWork> log,
            IBusDataAccess dataAccess,
            IFindSubscribedHandlers findSubscriptionHandler,
            IFindSubscribedPipelineSteps findPipelineSteps)
        {
            _reader = reader;
            _counters = counters;
            _log = log;
            _dataAccess = dataAccess;
            _findSubscriptionHandlers = findSubscriptionHandler;
            _findPipelineSteps = findPipelineSteps;
        }

        public Guid SubscriberId { get; set; }

        /// <summary>
        /// Actually does the work of processing a subscription message
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoWork()
        {
            const string savepointName = "BeforeSubscriptionHandler";

            // get a message.
            var context = await _reader.GetNext(SubscriberId);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (context == null)
            {
                return false;
            }

            // we found a message to process.
            _log.SubscribedWork_ProcessingMessage(
                context.MessageData.MessageId,
                SubscriberId);
            var started = DateTime.UtcNow;
            try
            {
                _counters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _dataAccess.CreateSavepoint(savepointName);

                await InvokePipeline(context);

                // if nothing threw an exception, we can mark the message as processed.
                await _reader.Complete(context);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.SubscribedWork_MessageHandlerException(
                    context.MessageData.MessageId,
                    SubscriberId,
                    ex);
                _dataAccess.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _reader.Fail(context, ex);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            finally
            {
                _counters.FinishMessage(started);
            }
        }

        private async Task InvokePipeline(SubscribedContext context)
        {
            // todo build a chain of handlers
            // and invoke them
            var steps = _findPipelineSteps.FindSteps().OrderBy(s => s.Priority);

            var pipeline = new Pipeline<SubscribedContext>();
            foreach (var step in steps)
            {
                pipeline.Add(step);
            }

            var handlersStep = new SubscribedHandlersPipelineStep(_findSubscriptionHandlers);
            pipeline.Add(handlersStep);

            await pipeline.Invoke(context);
        }
    }
}
