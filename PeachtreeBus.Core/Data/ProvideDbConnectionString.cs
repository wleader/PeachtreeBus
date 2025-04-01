using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Data;

public class ProvideDbConnectionString(
    IBusConfiguration configuration)
    : IProvideDbConnectionString
{
    private readonly IBusConfiguration _configuration = configuration;
    public string GetDbConnectionString() => _configuration.ConnectionString;
}
