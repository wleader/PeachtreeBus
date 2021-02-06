using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading;

namespace PeachtreeBus.Counters
{
    [EventSource(Name = "PeachtreeBus")]
    public sealed class PeachtreeBusCounters : EventSource
    {
        private readonly static PeachtreeBusCounters EventSource = new PeachtreeBusCounters();

        private readonly PollingCounter _activeMessagesCounter;
        private readonly PollingCounter _failedMessagesCounter;
        private readonly EventCounter _messageTimeCounter;
        private readonly IncrementingEventCounter _attemptedmessages;
        private readonly IncrementingEventCounter _sagaBlocks;
        private readonly IncrementingEventCounter _retries;
        private readonly IncrementingEventCounter _errorMessages;
        private readonly IncrementingEventCounter _completeMessages;
        private readonly IncrementingEventCounter _sentMessages;
        private readonly IncrementingEventCounter _delayedMessages;

        private int _activeMessages;
        private int _failedMessages;

        private PeachtreeBusCounters() : base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {
            _activeMessagesCounter = new PollingCounter("messages-active", this, () => _activeMessages);
            _failedMessagesCounter = new PollingCounter("messages-failed", this, () => _failedMessages);
            _messageTimeCounter = new EventCounter("message-time", this);
            _attemptedmessages = new IncrementingEventCounter("messages-attempted", this);
            _sagaBlocks = new IncrementingEventCounter("saga-blocks", this);
            _retries = new IncrementingEventCounter("messages-retry", this);
            _errorMessages = new IncrementingEventCounter("messages-error", this);
            _completeMessages = new IncrementingEventCounter("messages-complete", this);
            _sentMessages = new IncrementingEventCounter("messages-sent", this);
            _delayedMessages = new IncrementingEventCounter("messages-delayed", this);
        }

        public static void StartMessage()
        {
            Interlocked.Increment(ref EventSource._activeMessages);
        }

        public static void FinishMessage(DateTime start)
        {
            Interlocked.Decrement(ref EventSource._activeMessages);
            var elapsed = DateTime.UtcNow.Subtract(start.ToUniversalTime()).TotalMilliseconds;
            EventSource._messageTimeCounter?.WriteMetric(elapsed);
            EventSource._attemptedmessages.Increment();
        }

        public static void SagaBlocked()
        {
            EventSource._sagaBlocks.Increment();
        }

        public static void RetryMessage()
        {
            EventSource._retries.Increment();
        }

        public static void CompleteMessage()
        {
            EventSource._completeMessages.Increment();
        }

        public static void ErrorMessage()
        {
            Interlocked.Increment(ref EventSource._failedMessages);
            EventSource._errorMessages.Increment();
        }

        public static void SentMessage()
        {
            EventSource._sentMessages.Increment();
        }

        public static void DelayMessage()
        {
            EventSource._delayedMessages.Increment();
        }
    }
}
