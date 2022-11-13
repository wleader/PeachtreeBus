using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.SimpleInjector
{
    public class SimpleInjectorScopeFactoryException :Exception
    {
        internal SimpleInjectorScopeFactoryException(string message)
            : base(message)
        {

        }
    }
}
