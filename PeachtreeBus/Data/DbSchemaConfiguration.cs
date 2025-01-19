namespace PeachtreeBus.Data
{

    /// <summary>
    /// Allows The DB Schema used by the bus to be provided through Dependency Injection.
    /// A singleton with this interfaces should be registered in the container.
    /// </summary>
    public interface IDbSchemaConfiguration
    {
        /// <summary>
        /// The Schema name ot use.
        /// </summary>
        SchemaName Schema { get; }
    }

    /// <summary>
    /// An implementation of IDbSchema that holds the value in memory.
    /// </summary>
    public class DbSchemaConfiguration(
        SchemaName? schema = null)
        : IDbSchemaConfiguration
    {
        /// <inheritdoc/>
        public SchemaName Schema { get; private set; } = schema ?? new("PeachtreeBus");
    }
}
