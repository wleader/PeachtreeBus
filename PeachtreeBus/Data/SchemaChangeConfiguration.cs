using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PeachtreeBus.Data
{
    public class SchemaChangeConfiguration<T> : IEntityTypeConfiguration<T> where T : class
    {
        private readonly string _schema;
        private readonly string _tableName;

        public SchemaChangeConfiguration(string schema, string tableName)
        {
            _schema = schema;
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<T> builder)
        {
            if (!string.IsNullOrWhiteSpace(_schema) && !string.IsNullOrWhiteSpace(_tableName))
            {
                builder.ToTable(_tableName, _schema);
            }
        }
    }
}
