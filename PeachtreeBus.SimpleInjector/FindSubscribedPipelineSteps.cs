using PeachtreeBus.Pipelines;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An implementation of IFindSubscribedPipelineSteps
    /// </summary>
    public class FindSubscribedPipelineSteps(
        IWrappedScope scope)
        : IFindSubscribedPipelineSteps
    {
        private readonly IWrappedScope _scope = scope;

        public IEnumerable<IPipelineStep<SubscribedContext>> FindSteps()
        {
            return _scope.GetAllInstances<ISubscribedPipelineStep>().ToList();
        }
    }
}
