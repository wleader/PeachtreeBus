using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
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

            _log.MessageFailed(message.GetType());
            try
            {
                // this can throw if dependency injection cannot provide
                // the requested type.
                handler = _handlerFactory.GetHandler();
            }
            catch (Exception ex)
            {
                _log.NoHandler(ex);
                return;
            }

            try
            {
                _dataAccess.CreateSavepoint(SavePointName);
                await handler.Handle(context, message, exception);
            }
            catch (Exception ex)
            {
                _log.HandlerThrow(handler.GetType(), message.GetType(), ex);
                _dataAccess.RollbackToSavepoint(SavePointName);
            }
        }
    }
}
