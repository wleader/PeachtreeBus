using System;

namespace PeachtreeBus.DatabaseSharing
{
    public class SharedDatabaseException : Exception
    {
        internal SharedDatabaseException(string message)
            : base(message)
        { }
    }
}
