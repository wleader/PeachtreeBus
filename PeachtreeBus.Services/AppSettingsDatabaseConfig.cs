using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.Services
{
    public class AppSettingsDatabaseConfig : IProvideDbConnectionString
    {
        private readonly IConfiguration _configuration;
        public AppSettingsDatabaseConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetDbConnectionString()
        {
            var result = _configuration.GetConnectionString("PeachtreeBus");
            return result;
        }
    }
}
