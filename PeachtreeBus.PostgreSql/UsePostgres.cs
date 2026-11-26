using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus;

public class UsePostgres : IRegisterBusDataAccess
{
    public void Register(IRegistrationProvider provider)
    {
        provider.RegisterScoped<IBusDataAccess, PostgreSqlBusDataAccess>();
        provider.RegisterScoped<ISharedDatabase, NpgSqlSharedDatabase>();
        provider.RegisterScoped<INpgSqlConnectionFactory, NpgSqlConnectionFactory>();
        provider.RegisterScoped<ISharedDatabaseFactory, NpgSqlSharedDatabaseFactory>();
    }
}