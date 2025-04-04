﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Describes the unit of work that reads one subscribed messsage and processes it.
    /// </summary>
    public interface ISubscribedWork : IUnitOfWork
    {
        SubscriberId SubscriberId { get; set; }
    }

    /// <summary>
    /// A unit of work that reads one subscribed message and processes it.
    /// </summary>
    public class SubscribedWork(
        ISystemClock clock,
        ISubscribedReader reader,
        IMeters meters,
        ILogger<SubscribedWork> log,
        IBusDataAccess dataAccess,
        ISubscribedPipelineInvoker pipelineInvoker)
        : ISubscribedWork
    {
        private readonly ISystemClock _clock = clock;
        private readonly ISubscribedReader _reader = reader;
        private readonly IMeters _meters = meters;
        private readonly ILogger<SubscribedWork> _log = log;
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly ISubscribedPipelineInvoker _pipelineInvoker = pipelineInvoker;

        public SubscriberId SubscriberId { get; set; }

        /// <summary>
        /// Actually does the work of processing a subscription message
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoWork()
        {
            const string savepointName = "BeforeSubscriptionHandler";

            var started = _clock.UtcNow;

            // get a message.
            var context = await _reader.GetNext(SubscriberId);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (context == null)
            {
                return false;
            }

            using var activity = new ReceiveActivity(context, started);

            // we found a message to process.
            _log.SubscribedWork_ProcessingMessage(
                context.Data.MessageId,
                SubscriberId);

            try
            {
                _meters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _dataAccess.CreateSavepoint(savepointName);

                await _pipelineInvoker.Invoke(context);

                // if nothing threw an exception, we can mark the message as processed.
                await _reader.Complete(context);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.SubscribedWork_MessageHandlerException(
                    context.Data.MessageId,
                    SubscriberId,
                    ex);
                _dataAccess.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _reader.Fail(context, ex);
                activity.AddException(ex);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            finally
            {
                _meters.FinishMessage();
            }
        }
    }
}
