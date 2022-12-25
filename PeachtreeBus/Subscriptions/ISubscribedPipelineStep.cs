﻿using PeachtreeBus.Pipelines;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipelineStep : IPipelineStep<SubscribedContext>
    {
        public int Priority { get; }
    }
}
