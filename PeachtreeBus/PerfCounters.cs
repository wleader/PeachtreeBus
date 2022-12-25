using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace PeachtreeBus
{
    public interface IPerfCounters
    {
        void StartMessage();
        void FinishMessage(DateTime started);
        void SagaBlocked();
        void RetryMessage();
        void CompleteMessage();
        void FailMessage();
        void DelayMessage();
        void SentMessage();
    }

    [EventSource(Name = "PeachtreeBus")]
    public class PerfCounters : EventSource, IPerfCounters
    {
        private static readonly object LockObj = new();
        private static PerfCounters _instance = null;
        public static PerfCounters Instance()
        {
            lock (LockObj)
            {
                _instance ??= new PerfCounters();
            }
            return _instance;
        }


        // the number of messages currently being processed.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly PollingCounter _messagesActive;
        private int _messagesActiveCount = 0;

        // the number of messages sent to the Error Queue.
        // since the processes started.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly PollingCounter _messagesFailedTotal;
        private long _messagesErrorTotalCount = 0;

        // the amount of time spent on a messages. (Max, Min, Mean)
        private readonly EventCounter _messageTime;

        // how many messages did we try to process in the interval.
        private readonly IncrementingEventCounter _messagesAttemtped;

        // how many saga messages were not processed because
        // the saga was blocked in the interval.
        private readonly IncrementingEventCounter _sagaBlocks;

        // how many messages were scheduled for a retry because there
        // was an exception processing the message in the interval.
        private readonly IncrementingEventCounter _messageRetries;

        // how many messages were sent to the error queue in the interval.
        private readonly IncrementingEventCounter _messagesFailed;

        // how mny messages were sent to the complete queue in the interval.
        private readonly IncrementingEventCounter _messagesComplete;

        // how many messages were sent to the pending queue in the interval.
        private readonly IncrementingEventCounter _messagesSent;

        // how many messages were delayed because a needed resource was locked.
        // Saga blocking, etc.
        private readonly IncrementingEventCounter _messagesDelayed;

        private PerfCounters()
        {
            _messagesActive = new PollingCounter("messages-active", this, () => _messagesActiveCount);
            _messagesFailedTotal = new PollingCounter("messages-failed-total", this, () => _messagesErrorTotalCount);
            _messageTime = new EventCounter("message-time", this);
            _messagesAttemtped = new IncrementingEventCounter("messages-attempted", this);
            _sagaBlocks = new IncrementingEventCounter("saga-blocks", this);
            _messageRetries = new IncrementingEventCounter("messages-retry", this);
            _messagesFailed = new IncrementingEventCounter("messages-failed", this);
            _messagesComplete = new IncrementingEventCounter("messages-complete", this);
            _messagesSent = new IncrementingEventCounter("messages-sent", this);
            _messagesDelayed = new IncrementingEventCounter("messages-delayed", this);
        }

        /// <summary>
        /// counts a message that is completed
        /// </summary>
        public void CompleteMessage()
        {
            _messagesComplete.Increment();
        }

        /// <summary>
        /// Counts a message that is delayed
        /// </summary>
        public void DelayMessage()
        {
            _messagesDelayed.Increment();
        }

        /// <summary>
        /// counts a message that has failed
        /// </summary>
        public void FailMessage()
        {
            Interlocked.Increment(ref _messagesErrorTotalCount);
            _messagesFailed.Increment();
        }

        /// <summary>
        /// Measures the time spent processing a message
        /// </summary>
        /// <param name="started"></param>
        public void FinishMessage(DateTime started)
        {
            Interlocked.Decrement(ref _messagesActiveCount);
            var elapsed = DateTime.UtcNow.Subtract(started.ToUniversalTime()).TotalMilliseconds;
            _messageTime.WriteMetric(elapsed);
            _messagesAttemtped.Increment();
        }

        /// <summary>
        /// Counts a message that will be retried.
        /// </summary>
        public void RetryMessage()
        {
            _messageRetries.Increment();
        }

        /// <summary>
        /// Counts a saga that is blocked
        /// </summary>
        public void SagaBlocked()
        {
            _sagaBlocks.Increment();
        }

        /// <summary>
        /// counts a message being started.
        /// </summary>
        public void StartMessage()
        {
            Interlocked.Increment(ref _messagesActiveCount);
        }

        /// <summary>
        /// Counts messages sent.
        /// </summary>
        public void SentMessage()
        {
            _messagesSent.Increment();
        }

        /// <summary>
        /// sets totals back to zero.
        /// </summary>
        public void Reset()
        {
            // this is really only here for testing purposes
            // which is why it is not exposed on the interface.
            _messagesErrorTotalCount = 0;
            _messagesActiveCount = 0;
        }
    }
}
