using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public class MsSqlInitializeDatabase(
    IMsSqlTestSettings settings,
    IManageTestDatabase manager)
    : BaseInitializeTestDatabase(settings, manager)
{
    // this could probably be made into configuration?
    private static readonly DockerImage Image = new("mcr.microsoft.com/mssql/server:2022-latest");

    protected override string DatabaseType => "MicrosoftSQL";

    protected override string BuildTestConnectionString()
    {
        var server = Container!.GetConnectionString();
        var result = new SqlConnectionStringBuilder(server)
        {
            InitialCatalog = Settings.DatabaseName,
        };
        return result.ConnectionString;
    }

    protected override IDatabaseContainer BuildContainer()
    {
        var result = new MsSqlBuilder(Image)
            .WithReuse(settings.ReuseContainer)
            .WithLabel("reuse-id", "peachtree-bus-tests-mssql")
            // if you change the password, you will need to remove the old persisted data,
            // because the persisted data will contain the old password.
            // therefore, a hard-coded password avoids changes.
            .WithPassword("YourStrong(!)Password");

        if (settings.PersistContainerData)
        {
            result = result.WithVolumeMount("peachtree-bus-tests-mssql-persistent-data", "/var/opt/mssql");
        }

        return result.Build();
    }
}