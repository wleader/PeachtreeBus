﻿using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    /// <summary>
    /// An implementation of IFindSubscribedPipelineSteps
    /// </summary>
    public class FindSubscribedPipelineSteps : IFindSubscribedPipelineSteps
    {
        private readonly IWrappedScope _scope;
        public FindSubscribedPipelineSteps(IWrappedScope scope)
        {
            _scope = scope;
        }

        public IEnumerable<ISubscribedPipelineStep> FindSteps()
        {
            return _scope.GetAllInstances<ISubscribedPipelineStep>().ToList();
        }
    }
}