using Microsoft.Extensions.Logging;
using PeachtreeBus.Queues;

namespace PeachtreeBus
{
    internal static class Events
    {
        internal static EventId DapperDataAcess_DataAccessError =
            new(42001, nameof(DapperDataAcess_DataAccessError));

        internal static EventId QueueReader_HeaderNotDeserializable =
            new(42002, nameof(QueueReader_HeaderNotDeserializable));

        internal static EventId QueueReader_BodyNotDeserializable =
            new(42003, nameof(QueueReader_BodyNotDeserializable));

        internal static EventId QueueReader_MessageClassNotRecognized =
            new(42004, nameof(QueueReader_MessageClassNotRecognized));

        internal static EventId QueueReader_MessageExceededMaxRetries =
            new(42005, nameof(QueueReader_MessageExceededMaxRetries));

        internal static EventId QueueReader_MessageWillBeRetried =
            new(42006, nameof(QueueReader_MessageWillBeRetried));

        internal static EventId QueueReader_SavingSagaData =
            new(42007, nameof(QueueReader_SavingSagaData));

        internal static EventId QueueReader_LoadingSagaData =
            new(42007, nameof(QueueReader_LoadingSagaData));

        internal static EventId BaseThread_ThreadStart =
            new(42008, nameof(BaseThread_ThreadStart));

        internal static EventId BaseThread_ThreadStop=
            new(42009, nameof(BaseThread_ThreadStop));

        internal static EventId BaseThread_ThreadError =
            new(42010, nameof(BaseThread_ThreadError));

        internal static EventId SubscribedWork_ProcessingMessage =
            new(42011, nameof(SubscribedWork_ProcessingMessage));

        internal static EventId SubscribedWork_MessageHandlerException =
            new(42012, nameof(SubscribedWork_MessageHandlerException));

        internal static EventId SubscribedReader_MessageExceededMaxRetries =
            new(42013, nameof(SubscribedReader_MessageExceededMaxRetries));

        internal static EventId SubscribedReader_HeaderNotDeserializable =
            new(42014, nameof(SubscribedReader_HeaderNotDeserializable));

        internal static EventId SubscribedReader_BodyNotDeserializable =
            new(42015, nameof(SubscribedReader_BodyNotDeserializable));

        internal static EventId SubscribedReader_MessageClassNotRecognized =
            new(42016, nameof(SubscribedReader_MessageClassNotRecognized));

        internal static EventId SubscribedReader_MessageWillBeRetried =
            new(42017, nameof(SubscribedReader_MessageWillBeRetried));

        internal static EventId QueueWork_ProcessingMessage =
            new(42018, nameof(QueueWork_ProcessingMessage));

        internal static EventId QueueWork_LoadingSaga =
            new(42019, nameof(QueueWork_LoadingSaga));

        internal static EventId QueueWork_SagaBlocked =
            new(42020, nameof(QueueWork_SagaBlocked));

        internal static EventId QueueWork_InvokeHandler =
            new(42021, nameof(QueueWork_InvokeHandler));

        internal static EventId QueueWork_SagaSaved =
            new(42021, nameof(QueueWork_SagaSaved));

        internal static EventId QueueWork_HandlerException =
            new(42022, nameof(QueueWork_HandlerException));
    }
}
