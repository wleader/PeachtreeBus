using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    public abstract class FindPipelineSteps<TContext, TPipelineStep>(
        IWrappedScope scope) : IFindPipelineSteps<TContext>
        where TPipelineStep : class, IPipelineStep<TContext>
    {
        private readonly IWrappedScope _scope = scope;

        public IEnumerable<IPipelineStep<TContext>> FindSteps()
        {
            return _scope.GetAllInstances<TPipelineStep>().ToList();
        }
    }

    public class FindQueuedPipelineSteps(
        IWrappedScope scope)
        : FindPipelineSteps<IQueueContext, IQueuePipelineStep>(scope)
        , IFindQueuePipelineSteps;

    public class FindSubscribedPipelineSteps(
        IWrappedScope scope)
        : FindPipelineSteps<ISubscribedContext, ISubscribedPipelineStep>(scope)
        , IFindSubscribedPipelineSteps;

    public class FindPublishPipelineSteps(
        IWrappedScope scope)
        : FindPipelineSteps<IPublishContext, IPublishPipelineStep>(scope)
        , IFindPublishPipelineSteps;

    public class FindSendPipelineSteps(
        IWrappedScope scope)
        : FindPipelineSteps<ISendContext, ISendPipelineStep>(scope)
        , IFindSendPipelineSteps;
}
