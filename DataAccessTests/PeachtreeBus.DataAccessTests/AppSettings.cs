using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseTestingShared;

namespace PeachtreeBus.DataAccessTests;

public static class AppSettings
{
    public static DbConnectionString InvalidDatabase { get; private set; }

    public static void Initialize()
    {
        TestSettings.Initialize();
        InvalidDatabase = TestSettings.ConfigurationRoot.GetConnectionString("InvalidDatabase");
    }
}


