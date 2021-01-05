using SimpleInjector;
using System;
using System.Linq;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Gets a list of registered type from the container that impliment the specified interface.
        /// </summary>
        /// <typeparam name="T">The interface to find.</typeparam>
        /// <param name="container">The container to search.</param>
        /// <returns>An array of type implementing the desired interface.</returns>
        public static Type[] FindTypesThatImplement<T>(this Container container)
        {
            return container
                .GetRootRegistrations()
                .Where(ip => ip.ImplementationType.GetInterfaces().Contains(typeof(T)))
                .Select(ip => ip.ImplementationType)
                .ToArray();
        }
    }
}
