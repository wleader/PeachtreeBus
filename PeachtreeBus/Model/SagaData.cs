using System;

namespace PeachtreeBus.Model
{

    /// <summary>
    /// Represents a row in the saga data table.
    /// </summary>
    public class SagaData
    {
        /// <summary>
        /// Primary key, Identity
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// A Uniuque ID. Maybe redundant, but good for logging.
        /// </summary>
        public virtual Guid SagaId { get; set; }

        /// <summary>
        /// What instance of the Saga class is the data related to.
        /// </summary>
        public virtual string Key { get; set; }

        /// <summary>
        /// The serialized Saga Data.
        /// </summary>
        public virtual string Data { get; set; }

        public virtual bool Blocked { get; set; }
    }
}
