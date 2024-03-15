using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors
{
    public interface IQueueFailures
    {
        Task Failed(QueueContext context, object message, Exception exception);
    }

    public class QueueFailures(
        ILogger<QueueFailures> log,
        IBusDataAccess dataAccess,
        IFailedQueueMessageHandlerFactory handlerFactory)
        : IQueueFailures
    {
        private readonly ILogger<QueueFailures> _log = log;
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IFailedQueueMessageHandlerFactory _handlerFactory = handlerFactory;

        public async Task Failed(QueueContext context, object message, Exception exception)
        {
            const string SavePointName = "BeforeHandleFailed";

            IHandleFailedQueueMessages handler;

            _log.QueueFailures_MessageFailed(message.GetType());
            try
            {
                // this can throw if dependency injection cannot provide
                // the requested type.
                handler = _handlerFactory.GetHandler();
            }
            catch (Exception ex)
            {
                _log.QueueFailures_NoHandler(ex);
                return;
            }

            try
            {
                _dataAccess.CreateSavepoint(SavePointName);
                await handler.Handle(context, message, exception);
            }
            catch (Exception ex)
            {
                _log.QueueFailures_HandlerThrow(handler.GetType(), message.GetType(), ex);
                _dataAccess.RollbackToSavepoint(SavePointName);
            }
        }
    }
}
