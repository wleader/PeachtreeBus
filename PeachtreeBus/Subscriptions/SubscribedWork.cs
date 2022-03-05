using PeachtreeBus.Data;
using System;
using System.Collections.Generic;
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
        private readonly ILog<SubscribedWork> _log;
        private readonly IBusDataAccess _dataAccess;
        private readonly IFindSubscribedHandlers _findSubscriptionHandlers;

        public SubscribedWork(
            ISubscribedReader reader,
            IPerfCounters counters,
            ILog<SubscribedWork> log,
            IBusDataAccess dataAccess,
            IFindSubscribedHandlers findSubscriptionHandler)
        {
            _reader = reader;
            _counters = counters;
            _log = log;
            _dataAccess = dataAccess;
            _findSubscriptionHandlers = findSubscriptionHandler;
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
            var subsriptionContext = await _reader.GetNext(SubscriberId);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (subsriptionContext == null)
            {
                return false;
            }

            // we found a message to process.
            _log.Debug($"Processing {subsriptionContext.MessageData.MessageId}");
            var started = DateTime.UtcNow;
            try
            {
                _counters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _dataAccess.CreateSavepoint(savepointName);

                // determine what type of message it is.
                var messageType = Type.GetType(subsriptionContext.Headers.MessageClass);
                if (messageType == null)
                {
                    throw new ApplicationException($"Message {subsriptionContext.MessageData.MessageId}  as a message class of {subsriptionContext.Headers.MessageClass} which was not a recognized type.");
                }

                // Get the message handlers for this message type from the Dependency Injection container.
                // the list will contain both regular handlers and sagas.
                // if a message has mulitple handlers, we'll get multiple handlers.
                var method = typeof(IFindSubscribedHandlers).GetMethod("FindHandlers");
                var genericMethod = method.MakeGenericMethod(messageType);
                var handlers = genericMethod.Invoke(_findSubscriptionHandlers, null);
                var castHandlers = (handlers as IEnumerable<object>).ToArray();

                // sanity check that the Depenency Injection container found at least one handler.
                // we shouldn't process a message that has no handlers.
                if (castHandlers.Length < 1)
                {
                    throw new ApplicationException($"There were no message handlers for {subsriptionContext.Headers.MessageClass}.");
                }

                // invoke each of the handlers.
                foreach (var handler in castHandlers)
                {
                    // determine if this handler is a saga.
                    var handlerType = handler.GetType();

                    // find the right method on the handler.
                    var parameterTypes = new[] { typeof(SubscribedContext), messageType };
                    var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);

                    // Invoke and await the method.
                    // should it have a seperate try-catch around this and treat it differently?
                    // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                    // does that mater for the retry?
                    {
                        var taskObject = handleMethod.Invoke(handler, new object[] { subsriptionContext, subsriptionContext.Message });
                        var castTask = taskObject as Task;
                        await castTask;
                    }
                }

                // if nothing threw an exception, we can mark the message as processed.
                await _reader.Complete(subsriptionContext);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.Warn($"There was an execption processing the message. {ex}");
                _dataAccess.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _reader.Fail(subsriptionContext, ex);
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
