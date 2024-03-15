namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// Defines a class that holds an instance of an ISharedDatabase.
    /// </summary>
    /// <remarks>
    /// This exists as a way to trick Dependency Injection scopes into
    /// Sharing the ISharedDatabase instance.
    /// The process is as follows:
    /// ISharedDatabase is registered with the DI Container
    ///     The registration is such that when the container wants an ISharedDatabase it will:
    ///         Get an ISharedDatabaseProvider from the container.
    ///         If the ISharedDatabaseProvider has an ISharedDatabase, use that instance.
    ///         If the ISharedDatabaseProvider does not have an ISharedDatabase, create a new ISharedDatabase. 
    /// An Outer Scope is started.
    /// An ISharedDatabase is created in the outer scope.
    /// An Inner Scope is started.
    /// An ISharedDatabaseProvider is created in the inner scope.
    /// The ISharedDatabaseProvider is given a reference to the ISharedDatabase from the outer scope.
    /// The Inner Scope can then be used to create objects, and when it does, the DI container will
    /// provider the ISharedDatabase from the outer scope to any objects in the inner scope that needs it.
    /// </remarks>
    public interface IShareObjectsBetweenScopes
    {
        public ISharedDatabase? SharedDatabase { get; set; }
    }
}
