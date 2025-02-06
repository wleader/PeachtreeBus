using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Define an task that will continuously read messages from a queue and attempt to process them
    /// </summary>
    public interface IQueueThread : IThread { }

    /// <summary>
    /// Reads messages from the queue and attempts to process them.
    /// </summary>
    public class QueueThread : BaseThread, IQueueThread
    {
        private readonly IQueueWork _queueWork;

        public QueueThread(IProvideShutdownSignal shutdown,
            IBusDataAccess dataAccess,
            ILogger<QueueThread> log,
            IQueueWork queueWork,
            IBusConfiguration config)
            : base("Queue", 100, log, dataAccess, shutdown)
        {
            _queueWork = queueWork;
            _queueWork.QueueName = UnreachableException.ThrowIfNull(config.QueueConfiguration,
                message: "QueueConfiguration was not provided. Queues must be configured to create a QueueThread.")
                .QueueName;
        }

        public override async Task<bool> DoUnitOfWork()
        {
            return await _queueWork.DoWork();
        }
    }
}
