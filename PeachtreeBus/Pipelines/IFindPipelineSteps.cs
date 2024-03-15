using System.Collections.Generic;

namespace PeachtreeBus.Pipelines
{
    public interface IFindPipelineSteps<TContext>
    {
        IEnumerable<IPipelineStep<TContext>> FindSteps();
    }
}
