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
        IHandleFailedQueueMessages handleFailed)
        : IQueueFailures
    {
        public async Task Failed(QueueContext context, object message, Exception exception)
        {
            const string SavePointName = "BeforeHandleFailed";
            log.MessageFailed(message.GetType());
            try
            {
                dataAccess.CreateSavepoint(SavePointName);
                await handleFailed.Handle(context, message, exception);
            }
            catch (Exception ex)
            {
                log.HandlerThrow(handleFailed.GetType(), message.GetType(), ex);
                dataAccess.RollbackToSavepoint(SavePointName);
            }
        }
    }
}
