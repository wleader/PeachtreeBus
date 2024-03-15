using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseSharing;
using System;

namespace PeachtreeBus.Services
{
    /// <summary>
    /// An implementation of IProvideDbConnectionString that reads it from
    /// a Microsoft.Extensions.Configuration.IConfiguration
    /// </summary>
    public class AppSettingsDatabaseConfig : IProvideDbConnectionString
    {
        private readonly IConfiguration _configuration;
        public AppSettingsDatabaseConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetDbConnectionString()
        {
            var result = _configuration.GetConnectionString("PeachtreeBus")
                ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");
            return result;
        }
    }
}
