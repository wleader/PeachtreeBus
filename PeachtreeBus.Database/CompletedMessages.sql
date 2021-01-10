-- reference definition of a completed messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[QueueName_CompletedMessages]
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
