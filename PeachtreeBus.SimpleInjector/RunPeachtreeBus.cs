﻿using SimpleInjector;
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
        public static Task StartPeachtreeBusMessageProcessor (this Container container, string queueName, int concurrency)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < concurrency; i++)
            {
                var scope = container.GetInstance<IScopeManager>();
                tasks.Add(scope.GetInstance<IMessageProcessor>().Run(queueName));
            }
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Starts the message processors, and the queue cleaner, and any other instances of IRunOnIntervalTask.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="queueId">The queue the message processors will read from.</param>
        /// <param name="concurrency">the number of message processors to create.</param>
        /// <returns>A task that ends when all the messages processors and interval task have shut down.</returns>
        public static Task StartPeachtreeBus(this Container container, string queueName, int concurrency)
        {
            return Task.WhenAll(StartPeachtreeBusMessageProcessor(container, queueName, concurrency),
                StartPeachtreeBusIntervalTasks(container));
        }

    }
}
