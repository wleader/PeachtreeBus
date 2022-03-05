using System;

namespace PeachtreeBus.Cleaners
{

    /// <summary>
    /// Describes the configuration needed by BaseCleanupWork
    /// </summary>
    public interface IBaseCleanupConfiguration
    {
        /// <summary>
        /// The maximum number of messages to clean in a single operation.
        /// Used to keep the Database Transaction from begin too large.
        /// </summary>
        int MaxDeleteCount { get; }

        /// <summary>
        /// Turns cleaning Completed Messages on and off.
        /// </summary>
        bool CleanCompleted { get; }

        /// <summary>
        /// Turns Cleaning Failed messages on and off.
        /// </summary>
        bool CleanFailed { get; }

        /// <summary>
        /// Time since the message completed or failed to be eligble for cleanup.
        /// </summary>
        TimeSpan AgeLimit { get; }

        /// <summary>
        /// An amount of time for the cleaning thread to idle before cleaning again.
        /// </summary>
        TimeSpan Interval { get; }
    }

    /// <summary>
    /// A default implementatin of IBaseCleanupConfiguration
    /// </summary>
    public class BaseCleanupConfiguration : IBaseCleanupConfiguration
    {
        public int MaxDeleteCount { get; set; }
        public bool CleanCompleted { get; set; }
        public bool CleanFailed { get; set; }
        public TimeSpan AgeLimit { get; set; }
        public TimeSpan Interval { get; set; }

        public BaseCleanupConfiguration(int maxDeleteCount, bool cleanCompleted, bool cleanFailed,
            TimeSpan ageLimit, TimeSpan interval)
        {
            MaxDeleteCount = maxDeleteCount;
            CleanCompleted = cleanCompleted;
            CleanFailed = cleanFailed;
            AgeLimit = ageLimit;
            Interval = interval;
        }
    }
}
