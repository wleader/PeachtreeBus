namespace PeachtreeBus.Data
{

    /// <summary>
    /// Allows The DB Schema used by the bus to be provided through Dependency Injection.
    /// A singleton with this interfaces should be registered in the container.
    /// </summary>
    public interface IDbSchema
    {
        /// <summary>
        /// The Schema name ot use.
        /// </summary>
        string Schema { get; }
    }

    /// <summary>
    /// An implementation of IDbSchema that holds the value in memory.
    /// </summary>
    public class DefaultDbSchema : IDbSchema
    {
        /// <inheritdoc/>
        public string Schema { get; private set; }
        public DefaultDbSchema( string schema = "PeachtreeBus")
        {
            Schema = schema;
        }
    }
}
