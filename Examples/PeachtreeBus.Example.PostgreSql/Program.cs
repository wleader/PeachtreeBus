using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.Example.Data;

namespace PeachtreeBus.Example;

public class Program : BaseProgram
{
    public static async Task Main(string[] args)
    {
        var program = new Program();
        await program.Run(args);
    }

    protected override IRegisterBusDataAccess UseDatabase() => new UsePostgres();

    protected override void RegisterComponents(IServiceCollection services)
    {
        // add application specific services
        services.AddScoped<IExampleDataAccess, PostgreSqlDataAccess>();
    }

    protected override string ServiceName => "PeachtreeBus-Example-PostgresSql";

    protected override string ConnectionStringName => "PeachtreeBusPostgreSql";
}