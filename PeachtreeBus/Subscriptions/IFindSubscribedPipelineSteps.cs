using System.Collections.Generic;

namespace PeachtreeBus.Subscriptions
{
    public interface IFindSubscribedPipelineSteps
    {
        IEnumerable<ISubscribedPipelineStep> FindSteps();
    }
}
