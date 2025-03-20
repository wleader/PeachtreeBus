using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues;

public interface ISendPipelineStep : IPipelineStep<ISendContext>;
