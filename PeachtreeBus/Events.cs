using Microsoft.Extensions.Logging;

namespace PeachtreeBus
{
    internal static class Events
    {
        private const int BaseId = 42000;

        internal static EventId DapperDataAcess_DataAccessError =
            new(BaseId + 1, nameof(DapperDataAcess_DataAccessError));

        internal static EventId QueueReader_HeaderNotDeserializable =
            new(BaseId + 2, nameof(QueueReader_HeaderNotDeserializable));

        internal static EventId QueueReader_BodyNotDeserializable =
            new(BaseId + 3, nameof(QueueReader_BodyNotDeserializable));

        internal static EventId QueueReader_MessageClassNotRecognized =
            new(BaseId + 4, nameof(QueueReader_MessageClassNotRecognized));

        internal static EventId QueueReader_MessageExceededMaxRetries =
            new(BaseId + 5, nameof(QueueReader_MessageExceededMaxRetries));

        internal static EventId QueueReader_MessageWillBeRetried =
            new(BaseId + 6, nameof(QueueReader_MessageWillBeRetried));

        internal static EventId QueueReader_SavingSagaData =
            new(BaseId + 7, nameof(QueueReader_SavingSagaData));

        internal static EventId QueueReader_LoadingSagaData =
            new(BaseId + 7, nameof(QueueReader_LoadingSagaData));

        internal static EventId BaseThread_ThreadStart =
            new(BaseId + 8, nameof(BaseThread_ThreadStart));

        internal static EventId BaseThread_ThreadStop=
            new(BaseId + 9, nameof(BaseThread_ThreadStop));

        internal static EventId BaseThread_ThreadError =
            new(BaseId + 10, nameof(BaseThread_ThreadError));

        internal static EventId SubscribedWork_ProcessingMessage =
            new(BaseId + 11, nameof(SubscribedWork_ProcessingMessage));

        internal static EventId SubscribedWork_MessageHandlerException =
            new(BaseId + 12, nameof(SubscribedWork_MessageHandlerException));

        internal static EventId SubscribedReader_MessageExceededMaxRetries =
            new(BaseId + 13, nameof(SubscribedReader_MessageExceededMaxRetries));

        internal static EventId SubscribedReader_HeaderNotDeserializable =
            new(BaseId + 14, nameof(SubscribedReader_HeaderNotDeserializable));

        internal static EventId SubscribedReader_BodyNotDeserializable =
            new(BaseId + 15, nameof(SubscribedReader_BodyNotDeserializable));

        internal static EventId SubscribedReader_MessageClassNotRecognized =
            new(BaseId + 16, nameof(SubscribedReader_MessageClassNotRecognized));

        internal static EventId SubscribedReader_MessageWillBeRetried =
            new(BaseId + 17, nameof(SubscribedReader_MessageWillBeRetried));

        internal static EventId QueueWork_ProcessingMessage =
            new(BaseId + 18, nameof(QueueWork_ProcessingMessage));

        internal static EventId QueueWork_LoadingSaga =
            new(BaseId + 19, nameof(QueueWork_LoadingSaga));

        internal static EventId QueueWork_SagaBlocked =
            new(BaseId + 20, nameof(QueueWork_SagaBlocked));

        internal static EventId QueueWork_InvokeHandler =
            new(BaseId + 21, nameof(QueueWork_InvokeHandler));

        internal static EventId QueueWork_SagaSaved =
            new(BaseId + 21, nameof(QueueWork_SagaSaved));

        internal static EventId QueueWork_HandlerException =
            new(BaseId + 22, nameof(QueueWork_HandlerException));
    }
}
