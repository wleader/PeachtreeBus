namespace PeachtreeBus.DatabaseSharing
{

    public interface ISharedDatabaseFactory
    {
        ISharedDatabase GetSharedDatabase();
    }

    public class SharedDatabaseFactory : ISharedDatabaseFactory
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        public SharedDatabaseFactory(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public ISharedDatabase GetSharedDatabase()
        {
            return new SharedDatabase(_sqlConnectionFactory.GetConnection());
        }
    }
}
