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
            ILog<QueueThread> log,
            IQueueWork queueWork,
            IQueueConfiguration config)
            : base( "Queue", 100, log, dataAccess, shutdown )
        {
            _queueWork = queueWork;
            _queueWork.QueueName = config.QueueName;
        }

        public override Task<bool> DoUnitOfWork()
        {
            return _queueWork.DoWork();
        }
    }
}
