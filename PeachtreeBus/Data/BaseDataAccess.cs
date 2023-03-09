using System.Linq;

namespace PeachtreeBus.Data
{
    public abstract class BaseDataAccess
    {
        const string SafeChars = "abcdefghijklmnopqrstuvwxyz0123456789";

        protected const string SchemaUnsafe = "The schema name contains not allowable characters.";
        protected const string QueueNameUnsafe = "The queue name contains not allowable characters.";

        protected bool IsUnsafe(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return value.ToLower().Any(c => !SafeChars.Contains(c));
        }
    }
}
