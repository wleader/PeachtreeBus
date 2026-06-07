using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// A class for testing the content of the subscriptions table.
    /// The main project model doesn't need this so it's only in the test code.
    /// </summary>
    public class SubscriptionsRow
    {
        public virtual Identity Id { get; set; }
        public virtual SubscriberId SubscriberId { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual UtcDateTime ValidUntil { get; set; }
    }
}
