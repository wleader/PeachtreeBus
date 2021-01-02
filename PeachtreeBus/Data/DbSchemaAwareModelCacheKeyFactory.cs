using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PeachtreeBus.Data
{
    public class DbSchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            return new
            {
                Type = context.GetType(),
                Schema = context is EFDataContext schema
                       ? schema.Schema
                       : null
            };
        }
    }
}
