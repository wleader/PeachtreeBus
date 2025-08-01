﻿using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

/// <summary>
///  Adds a message to a queue using an IBusDataAccess.
/// </summary>
public class QueueWriter(
    ISystemClock clock,
    ISendPipelineInvoker pipelineInvoker,
    IClassNameService classNameService)
    : IQueueWriter
{
    private readonly ISystemClock _clock = clock;
    private readonly ISendPipelineInvoker pipelineInvoker = pipelineInvoker;
    private readonly IClassNameService _classNameService = classNameService;

    public async Task WriteMessage(
        QueueName queueName,
        IQueueMessage message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null,
        bool newConveration = false)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var context = new SendContext()
        {
            Data = new()
            {
                NotBefore = notBefore ?? _clock.UtcNow,
                Headers = new()
                {
                    MessageClass = _classNameService.GetClassNameForType(message.GetType()),
                    UserHeaders = userHeaders ?? []
                },
                MessageId = UniqueIdentity.New(),
                Body = default!,
                Enqueued = _clock.UtcNow,
                Priority = priority,
            },
            Destination = queueName,
            Message = message,
            StartNewConversation = newConveration,
        };

        await pipelineInvoker.Invoke(context);
    }
}

