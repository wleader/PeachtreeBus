using System;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// A class for testing the content of the subscriptions table.
    /// The main project model doesn't need this so its only in the test code.
    /// </summary>
    public class SubscriptionsRow
    {
        public virtual long Id { get; set; }
        public virtual Guid SubscriberId { get; set; }
        public virtual string Category { get; set; } = string.Empty;
        public virtual DateTime ValidUntil { get; set; }
    }
}
