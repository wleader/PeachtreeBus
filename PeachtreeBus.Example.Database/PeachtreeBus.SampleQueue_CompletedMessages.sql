-- A table for completed messages from the queue named SampleQueue
CREATE TABLE [PeachtreeBus].[SampleQueue_CompletedMessages]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [MessageId] UNIQUEIDENTIFIER NOT NULL, 
    [NotBefore] DATETIME2 NOT NULL, 
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL, 
    [Retries] TINYINT NOT NULL DEFAULT 0,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL
)
