using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus;

public class UseMsSql : IRegisterBusDataAccess
{
    public void Register(IRegistrationProvider provider)
    {
        provider.RegisterScoped<IBusDataAccess, MsSqlBusDataAccess>();
        provider.RegisterScoped<MsSqlSharedDatabase, MsSqlSharedDatabase>();
        provider.RegisterScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        provider.RegisterScoped<ISharedDatabaseFactory, MsSqlSharedDatabaseFactory>();
    }
}