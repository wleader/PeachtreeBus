﻿using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Exceptions;

public class QueueMessageNoHandlerException(
    UniqueIdentity messageId,
    QueueName sourceQueue,
    Type messageType)
    : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which no handlers were found.")
{
    public UniqueIdentity MessageId { get; } = messageId;
    public Type MessageType { get; } = messageType;
    public QueueName SourceQueue { get; } = sourceQueue;
}
