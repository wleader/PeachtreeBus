using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus;

public class UsePostgres
{
    public void Register(IRegistrationProvider provider)
    {
        provider.RegisterScoped<IBusDataAccess, PostgreSqlBusDataAccess>();
        provider.RegisterScoped<ISharedDatabase, NpgSqlSharedDatabase>();
    }
}