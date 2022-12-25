using System;

namespace PeachtreeBus.SimpleInjector
{
    public class SimpleInjectorScopeFactoryException : Exception
    {
        internal SimpleInjectorScopeFactoryException(string message)
            : base(message)
        {

        }
    }
}
