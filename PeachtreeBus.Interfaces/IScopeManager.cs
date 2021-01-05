namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for a Dependency Injection Scope Manager.
    /// </summary>
    public interface IScopeManager
    {
        // Should be called each time an object in a new scope is needed.
        void Begin();

        // cleans up cached object instances for unused scopes.
        void DisposeAll();
    }
}
