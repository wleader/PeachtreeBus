using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// A class for testing the content of the subscriptions table.
    /// The main project model doesn't need this so its only in the test code.
    /// </summary>
    public class SubscriptionsRow
    {
        public virtual Identity Id { get; set; }
        public virtual SubscriberId SubscriberId { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual DateTime ValidUntil { get; set; }
    }
}
