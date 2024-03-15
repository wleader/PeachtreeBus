namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// See ISharedDatabaseProvider
    /// </summary>
    public class ShareObjectsBetweenScopes : IShareObjectsBetweenScopes
    {
        public ISharedDatabase? SharedDatabase { get; set; } = default;
    }
}
