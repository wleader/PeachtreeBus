using System.Runtime.CompilerServices;

namespace PeachtreeBus.Testing;

public static class UninitializedObjects
{
    /// <summary>
    /// Creates an object that has not been initialized.
    /// This is helpful for getting an instance of an object
    /// that does not have a public constructor, or requires
    /// constructor parameters that are not easily fulfilled.
    /// </summary>
    public static T Create<T>()
    {
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    }
}
