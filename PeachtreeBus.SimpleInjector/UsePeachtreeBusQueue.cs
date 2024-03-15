using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables handing of messages from a specific queue.
        /// Enables Sagas.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public static Container UsePeachtreeBusQueue(this Container container, string queueName)
        {
            // specify which queue this process is reading from
            container.RegisterSingleton(typeof(IQueueConfiguration), () => new QueueConfiguration(queueName));

            // register classes needed to service the message queue.
            container.RegisterSingleton(typeof(ISagaMessageMapManager), typeof(SagaMessageMapManager));
            container.Register(typeof(IFindQueueHandlers), typeof(FindQueueHandlers), Lifestyle.Scoped);
            container.Register(typeof(IFindQueuePipelineSteps), typeof(FindQueuedPipelineSteps), Lifestyle.Scoped);
            container.Register(typeof(IQueueThread), typeof(QueueThread), Lifestyle.Scoped);
            container.Register(typeof(IQueueWork), typeof(QueueWork), Lifestyle.Scoped);
            container.Register(typeof(IQueueReader), typeof(QueueReader), Lifestyle.Scoped);
            container.Register(typeof(IQueuePipelineInvoker), typeof(QueuePipelineInvoker), Lifestyle.Scoped);
            container.Register(typeof(IQueuePipelineFactory), typeof(QueuePipelineFactory), Lifestyle.Scoped);
            container.Register(typeof(IQueuePipeline), typeof(QueuePipeline), Lifestyle.Scoped);
            container.Register(typeof(IQueueHandlersPipelineStep), typeof(QueueHandlersPipelineStep), Lifestyle.Scoped);

            // register message handlers and sagas.
            // this finds types that impliment IHandleMessage<>.
            container.RegisterPeachtreeBusMessageHandlers();
            // register pipeline steps
            container.RegisterPeachtreeBusQueuedPipelineSteps();

            return container;
        }
    }
}
