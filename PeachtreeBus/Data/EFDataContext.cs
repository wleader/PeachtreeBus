using PeachtreeBus.Model;
using Microsoft.EntityFrameworkCore;
using PeachtreeBus.DatabaseSharing;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// Defines the DbContext Needed for EFBusDataAccess.
    /// </summary>
    public interface IEFContext : ISharedContext
    {
        /// <summary>
        /// A table of queue messages.
        /// </summary>
        DbSet<QueueMessage> QueueMessages { get; set; }

        /// <summary>
        /// A table of Saga data.
        /// </summary>
        DbSet<SagaData> SagaData { get; set; }

        string Schema { get; }
    }

    /// <summary>
    /// An implementation if IEFContext using Entity Framework Core.
    /// </summary>
    public class EFDataContext : SharedContext, IEFContext
    {
        /// <inheritdoc/>
        public DbSet<QueueMessage> QueueMessages { get; set; }
        /// <inheritdoc/>
        public DbSet<SagaData> SagaData { get; set; }
        
        public string Schema { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharedDatabase">A database connection that the bus can share with application code</param>
        public EFDataContext(ISharedDatabase sharedDatabase, IDbContextSchema schema) : base(sharedDatabase)
        {
            Schema = schema.Schema;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SchemaChangeConfiguration<QueueMessage>(Schema, nameof(QueueMessages)));
            modelBuilder.ApplyConfiguration(new SchemaChangeConfiguration<SagaData>(Schema, nameof(SagaData)));
        }
    }
}
