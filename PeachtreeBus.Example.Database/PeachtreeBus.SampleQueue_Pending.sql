-- A table for queue (pending) messages from the queue named SampleQueue
-- Messages in this table remain here temporarily after failure or completion before being moved,
-- but for the most part this table contains unprocessed messages.

CREATE TABLE [PeachtreeBus].[SampleQueue_Pending]
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY, 
    [MessageId] UNIQUEIDENTIFIER NOT NULL, 
    [NotBefore] DATETIME2 NOT NULL, 
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL, 
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL
)
GO

ALTER TABLE [PeachtreeBus].[SampleQueue_Pending] ADD DEFAULT ((0)) FOR [Retries]
GO
