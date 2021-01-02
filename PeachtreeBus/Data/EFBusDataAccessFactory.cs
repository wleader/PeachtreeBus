using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Data
{

    /// <summary>
    /// An IBusDataAccessFactory that creates an IBusDataAccess that Uses Entity Framework Core for data access.
    /// </summary>
    public class EFBusDataAccessFactory : IBusDataAccessFactory
    {
        // Holds a connection and transaction that can be shared between EF DbContexts.
        private readonly ISharedDatabase _sharedDatabase;
        private readonly IDbContextSchema _dbContextSchema;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">A connection to an SQL Database. May be shared with other data access code.</param>
        /// <param name="transactionHolder">Manages tranaction sharing between the Context and other data access code.</param>
        public EFBusDataAccessFactory(ISharedDatabase sharedDatabase,
            IDbContextSchema dbContextSchema)
        {
            _sharedDatabase = sharedDatabase;
            _dbContextSchema = dbContextSchema;
        }

        /// <summary>
        /// Gets an IBusDataAccess that uses Entity Framework Core.
        /// </summary>
        /// <returns>An IBusDataAccess that uses Entity Framework Core.</returns>
        public IBusDataAccess GetBusDataAccess()
        {
            return new EFBusDataAccess(new EFDataContext(_sharedDatabase, _dbContextSchema));
        }
    }
}
