namespace PeachtreeBus.Data
{

    /// <summary>
    /// Allows The DB Schema used by the bus to be provided through Dependency Injection.
    /// A singleton with this interfaces should be registered in the container.
    /// </summary>
    public interface IDbContextSchema
    {
        string Schema { get; }
    }

    public class DefaultDbContextSchema : IDbContextSchema
    {
        public string Schema { get; private set; }
        public DefaultDbContextSchema( string schema = "PeachtreeBus")
        {
            Schema = schema;
        }
    }
}
