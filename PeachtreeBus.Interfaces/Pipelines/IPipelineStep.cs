﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Pipelines;

/// <summary>
/// Defines the interface that users must implement to inject a pipeline step.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IPipelineStep<TContext>
{
    Task Invoke(TContext context, Func<TContext, Task> next);
    public int Priority { get; }
}

public interface IFindPipelineSteps<TContext>
{
    IEnumerable<IPipelineStep<TContext>> FindSteps();
}
