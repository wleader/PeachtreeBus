using PeachtreeBus.Data;
using System;

namespace PeachtreeBus.Sagas
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
        public virtual string Key { get; set; } = string.Empty;

        /// <summary>
        /// The serialized Saga Data.
        /// </summary>
        public virtual SerializedData Data { get; set; } = default;

        public virtual bool Blocked { get; set; }
    }
}
