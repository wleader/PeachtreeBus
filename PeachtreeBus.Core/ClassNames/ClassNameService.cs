using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.ClassNames;

public interface IClassNameService
{
    Type? GetTypeForClassName(ClassName name);
    ClassName GetClassNameForType(Type type);
}

public class ClassNameService : IClassNameService
{
    public ClassName GetClassNameForType(Type type) => type.GetClassName();
    public Type? GetTypeForClassName(ClassName name) => Type.GetType(name.Value);
}

public class CachedClassNameService(
    IClassNameService decorated)
    : IClassNameService
{
    private readonly ConcurrentDictionary<ClassName, Type?> _messageClassToType = [];
    private readonly ConcurrentDictionary<Type, ClassName> _typeToMessageClass = [];
    private readonly IClassNameService _decorated = decorated;

    public ClassName GetClassNameForType(Type type) =>
        _typeToMessageClass.GetOrAdd(type, _decorated.GetClassNameForType);

    public Type? GetTypeForClassName(ClassName messageClass) =>
        _messageClassToType.GetOrAdd(messageClass, _decorated.GetTypeForClassName);
}
