using PeachtreeBus.Data;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Registers an IDBSchem so that IBusDataAccess will know which schema in the database to use.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="schema">The name of the schema to use.</param>
        /// <returns></returns>
        public static Container UsePeachtreeBusDbSchema(this Container container, SchemaName schema)
        {
            container.RegisterSingleton(typeof(IDbSchemaConfiguration), () => new DbSchemaConfiguration(schema));
            return container;
        }
    }
}
