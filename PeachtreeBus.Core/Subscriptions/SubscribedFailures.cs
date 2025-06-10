using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedFailures
    {
        Task Failed(SubscribedContext context, object message, Exception exception);
    }

    public class SubscribedFailures(
        ILogger<SubscribedFailures> log,
        IBusDataAccess dataAccess,
        IHandleFailedSubscribedMessages handler)
        : ISubscribedFailures
    {
        public async Task Failed(SubscribedContext context, object message, Exception exception)
        {
            const string SavePointName = "BeforeHandleFailed";

            try
            {
                dataAccess.CreateSavepoint(SavePointName);
                await handler.Handle(context, message, exception);
            }
            catch (Exception ex)
            {
                log.HandlerThrow(handler.GetType(), message.GetType(), ex);
                dataAccess.RollbackToSavepoint(SavePointName);
            }
        }
    }
}
