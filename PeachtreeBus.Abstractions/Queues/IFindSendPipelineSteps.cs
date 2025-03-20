using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Queues;

public interface IFindSendPipelineSteps : IFindPipelineSteps<ISendContext>;
