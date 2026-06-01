using PeachtreeBus.Data;

namespace PeachtreeBus;

public class UseMsSql : IRegisterBusDataAccess
{
    public void Register(IRegistrationProvider provider)
    {
        provider.RegisterScoped<IBusDataAccess, DapperDataAccess>();
    }
}