using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.ScannableAssembly;

public abstract class PipelineStep<TContext>
{
    public int Priority { get; } = 0;
    public Task Invoke(TContext context, Func<TContext, Task> next) => next(context);
}

public class PublishPipelineStep1 : PipelineStep<IPublishContext>, IPublishPipelineStep;
public class PublishPipelineStep2 : PipelineStep<IPublishContext>, IPublishPipelineStep;
public class SendPipelineStep1 : PipelineStep<ISendContext>, ISendPipelineStep;
public class SendPipelineStep2 : PipelineStep<ISendContext>, ISendPipelineStep;
public class QueuePipelineStep1 : PipelineStep<IQueueContext>, IQueuePipelineStep;
public class QueuePipelineStep2 : PipelineStep<IQueueContext>, IQueuePipelineStep;
public class SubscribedPipelineStep1 : PipelineStep<ISubscribedContext>, ISubscribedPipelineStep;
public class SubscribedPipelineStep2 : PipelineStep<ISubscribedContext>, ISubscribedPipelineStep;
