using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Example.Subsciptions
{
    public static class Topics
    {
        public static readonly Topic Announcements = new(nameof(Announcements));
    }
}
