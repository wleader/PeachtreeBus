using PeachtreeBus.Pipelines;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class PipelineActivity<TContext> : IDisposable
{
    private readonly Activity? _activity;

    public PipelineActivity(IPipelineStep<TContext> step)
    {
        var type = step.GetType();

        _activity = DoNotTrace(type)
            ? null
            : ActivitySources.User.StartActivity(
                $"peachtreebus.pipeline {type.Name}",
                ActivityKind.Internal);

        _activity.AddPipelineType(type);
    }

    private static bool DoNotTrace(Type type)
    {
        // we don't want to trace the final step,
        // that's just part of the overhead from the user's
        // perspective, and clutters the trace view.

        var baseType = type.BaseType;
        return
            baseType is not null &&
            baseType.IsGenericType &&
            baseType.GetGenericTypeDefinition() == typeof(PipelineFinalStep<,>);
    }

    public void Dispose()
    {
        _activity?.Dispose();
        GC.SuppressFinalize(this);
    }
}
