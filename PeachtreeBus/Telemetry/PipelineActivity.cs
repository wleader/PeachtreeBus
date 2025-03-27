using PeachtreeBus.Pipelines;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class PipelineActivity : BaseActivity, IDisposable
{
    public PipelineActivity(Type pipelineType)
    {
        _activity = DoNotTrace(pipelineType)
            ? null
            : ActivitySources.User.StartActivity(
                $"peachtreebus.pipeline {pipelineType.Name}",
                ActivityKind.Internal)
                .AddPipelineType(pipelineType);
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
}
