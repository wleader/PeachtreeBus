using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Checks if a type is registered with the container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static bool IsRegistered<T>(this Container container)
        {
            return container.GetRegistration(typeof(T)) != null;
        }
    }
}