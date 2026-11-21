using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Npgsql;
using Testcontainers.PostgreSql;

namespace PeachtreeBus.DatabaseTesting.PostgreSql;

public class PostgresInitializeTestDatabase(ITestSettings settings, IManageTestDatabase manager) : BaseInitializeTestDatabase(settings, manager)
{
    private static readonly DockerImage Image = new("postgres:17-alpine");

    protected override string DatabaseType => "PostgreSQL";

    protected override IDatabaseContainer BuildContainer()
    {
        var result = new PostgreSqlBuilder(Image)
            .WithReuse(Settings.ReuseContainer)
            .WithLabel("reuse-id", "peachtree-bus-tests-pgsql");

        if (Settings.PersistContainerData)
        {
            result = result.WithVolumeMount("peachtree-bus-tests-pgsql-persistent-data", "/var/lib/postgresql/data");
        }

        return result.Build();
    }

    protected override string BuildTestConnectionString()
    {
        var result = new NpgsqlConnectionStringBuilder(Container!.GetConnectionString())
        {
            Database = Settings.DatabaseName
        };
        return result.ConnectionString;
    }
}