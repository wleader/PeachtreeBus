using PeachtreeBus.Exceptions;

namespace PeachtreeBus.SimpleInjector;

public class SimpleInjectorScopeFactoryException : PeachtreeBusException
{
    internal SimpleInjectorScopeFactoryException(string message) : base(message) { }
}
