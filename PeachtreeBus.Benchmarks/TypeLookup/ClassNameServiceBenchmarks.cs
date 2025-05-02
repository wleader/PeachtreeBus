using BenchmarkDotNet.Attributes;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Benchmarks.TypeLookup;

[MemoryDiagnoser]
public class ClassNameServiceBenchmarks
{
    private IClassNameService _basicLookup = default!;
    private IClassNameService _cachedLookup = default!;

    public class UserMessage : IQueueMessage;
    public static readonly Type UserMessageType = typeof(UserMessage);
    public static readonly ClassName UserMessageClassName = UserMessageType.GetClassName();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _basicLookup = new ClassNameService();
        _cachedLookup = new CachedClassNameService(new ClassNameService());

        // make sure the typical is in the cache.
        var userMessageType = typeof(UserMessage);
        var cn = _cachedLookup.GetClassNameForType(userMessageType);
        _ = _cachedLookup.GetTypeForClassName(cn);

        // jam some extra stuff into the cache so it actually has to do some work.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(a => a.GetTypes()).Take(1000);

        int count = 0;
        foreach (var t in types)
        {
            _ = _cachedLookup.GetTypeForClassName(_cachedLookup.GetClassNameForType(t));
            count++;
        }
        Console.WriteLine($"Cached {count} types.");
    }

    [Benchmark]
    public ClassName GetClassNameBasic()
    {
        return _basicLookup.GetClassNameForType(UserMessageType);
    }

    [Benchmark]
    public ClassName GetClassNameCached()
    {
        return _cachedLookup.GetClassNameForType(UserMessageType);
    }

    [Benchmark]
    public Type? GetTypeBasic()
    {
        return _basicLookup.GetTypeForClassName(UserMessageClassName);
    }

    [Benchmark]
    public Type? GetTypeCached()
    {
        return _cachedLookup.GetTypeForClassName(UserMessageClassName);
    }
}
