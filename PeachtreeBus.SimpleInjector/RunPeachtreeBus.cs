using SimpleInjector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Creates instances of IMesageProcessor and runs them.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="queueId">The queue the message processor will read from.</param>
        /// <param name="concurrency">The number of copies of the processor to create.</param>
        /// <returns>A task that ends when the message processors have all shut down.</returns>
        public static IList<Task> StartPeachtreeBusMessageProcessor (this Container container, string queueName, int concurrency)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < concurrency; i++)
            {
                var scope = container.GetInstance<IScopeManager>();
                var mp = scope.GetInstance<IMessageProcessor>().Run(queueName);
                tasks.Add(mp);
            }
            return tasks;
        }

        /// <summary>
        /// Starts the message processors, and the queue cleaner, and any other instances of IRunOnIntervalTask.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="queueId">The queue the message processors will read from.</param>
        /// <param name="concurrency">the number of message processors to create.</param>
        /// <returns>A task that ends when all the messages processors and interval task have shut down.</returns>
        public static IList<Task> StartPeachtreeBus(this Container container, string queueName, int concurrency)
        {
            var tasks = new List<Task>();
            tasks.AddRange(StartPeachtreeBusMessageProcessor(container, queueName, concurrency));
            tasks.AddRange(StartPeachtreeBusIntervalTasks(container));
            return tasks;
        }

    }
}
