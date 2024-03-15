namespace PeachtreeBus
{
    /// <summary>
    /// Headers stored with the message.
    /// </summary>
    public class Headers
    {
        /// <summary>
        /// the type the message was serialized from and to deserialize it to.
        /// </summary>
        public string MessageClass { get; set; } = string.Empty;

        /// <summary>
        /// Any exception details from a previous attempt to process the message.
        /// </summary>
        public string? ExceptionDetails { get; set; }
    }
}
