using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors
{
    public interface ISubscribedFailures
    {
        Task Failed(SubscribedContext context, object message, Exception exception);
    }

    public class SubscribedFailures(
        ILogger<SubscribedFailures> log,
        IBusDataAccess dataAccess,
        IFailedSubscribedMessageHandlerFactory handlerFactory)
        : ISubscribedFailures
    {
        private readonly ILogger<SubscribedFailures> _log = log;
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IFailedSubscribedMessageHandlerFactory _handlerFactory = handlerFactory;

        public async Task Failed(SubscribedContext context, object message, Exception exception)
        {
            const string SavePointName = "BeforeHandleFailed";

            IHandleFailedSubscribedMessages handler;

            _log.SubscribedFailures_MessageFailed(message.GetType());
            try
            {
                // this can throw if dependency injection cannot provide
                // the requested type.
                handler = _handlerFactory.GetHandler();
            }
            catch (Exception ex)
            {
                _log.SubscribedFailures_NoHandler(ex);
                return;
            }

            try
            {
                _dataAccess.CreateSavepoint(SavePointName);
                await handler.Handle(context, message, exception);
            }
            catch (Exception ex)
            {
                _log.SubscribedFailures_HandlerThrow(handler.GetType(), message.GetType(), ex);
                _dataAccess.RollbackToSavepoint(SavePointName);
            }
        }
    }
}
