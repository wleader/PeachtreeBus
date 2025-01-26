using PeachtreeBus.Data;

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
        public virtual Identity Id { get; set; }

        /// <summary>
        /// A Uniuque ID. Maybe redundant, but good for logging.
        /// </summary>
        public required virtual UniqueIdentity SagaId { get; set; }

        /// <summary>
        /// What instance of the Saga class is the data related to.
        /// </summary>
        public required virtual SagaKey Key { get; set; }

        /// <summary>
        /// The serialized Saga Data.
        /// </summary>
        public required virtual SerializedData Data { get; set; } = default;

        public required virtual bool Blocked { get; set; }
    }
}
