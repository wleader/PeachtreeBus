using System;

namespace PeachtreeBus.SourceGenerators.LoggerExtensions;

public interface IState
{
    bool ExcludeFromCodeCoverage { get; }
    string NamespaceUnderscored { get; }
    string EventFullName { get; }
    string CombinedId { get; }
    string GenericConstraint { get; }
    string? GenericArgs { get; }
    string ClassName { get; }
    void Initialize(AssemblyType data);
    void SetNamespace(NamespaceType data);
    void SetClass(ClassType data);
    void SetEvent(EventType data);
}

public class State : IState
{
    public int AssemblyId { get; private set; } = 0;
    public int NamespaceId { get; private set; } = 0;
    public int ClassId { get; private set; } = 0;
    public int EventId { get; private set; } = 0;
    public bool ExcludeFromCodeCoverage { get; private set; } = false;
    public string NamespaceUnderscored { get; private set; } = string.Empty;
    public string EventFullName { get; private set; } = string.Empty;
    public string CombinedId { get; private set; } = string.Empty;
    public string GenericConstraint { get; private set; } = string.Empty;
    public string? GenericArgs { get; private set; }
    public string ClassName { get; private set; } = string.Empty;

    public void Initialize(AssemblyType data)
    {
        AssemblyId = GetIdValue(data.assemblyId, "assemblyId", 1, 999);
        ExcludeFromCodeCoverage = data.exludeFromCodeCoverageSpecified && data.exludeFromCodeCoverage;
    }

    public void SetNamespace(NamespaceType data)
    {
        NamespaceId = GetIdValue(data.namespaceId, "namespaceId", 1, 99);
        NamespaceUnderscored = data.name?.Replace('.', '_') ?? string.Empty;
    }

    public void SetClass(ClassType data)
    {
        ClassId = GetIdValue(data.classId, "classId", 1, 99);
        ClassName = data.name;
        GenericConstraint = data.genericConstraint;
        GenericArgs = data.genericArgs is not null
            ? $"<{data.genericArgs}>"
            : null;
    }

    public void SetEvent(EventType data)
    {
        EventId = GetIdValue(data.eventId, "eventId", 1, 99);
        EventFullName = NamespaceUnderscored + "_" + ClassName + "_" + data.name;

        // this generates a unique event ID. This allows for
        // 999 assemblies in the solution.
        // 99 namespaces per assembly.
        // 99 classes per namesapce
        // 99 events per class.
        // honestly, if this is a problem for someone, they should probably split up their code.
        CombinedId = string.Format("{0}{1:00}{2:00}{3:00}", AssemblyId, NamespaceId, ClassId, EventId);
    }

    private int GetIdValue(int value, string name, int min, int max)
    {
        if (value < min || value > max)
            throw new ApplicationException($"{name} {value} is not within the range of {min}-{max} inclusive.");
        return value;
    }
}

