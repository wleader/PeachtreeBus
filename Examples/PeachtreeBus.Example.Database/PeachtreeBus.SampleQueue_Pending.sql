-- A table for queue (pending) messages from the queue named SampleQueue
-- Messages in this table remain here temporarily after failure or completion before being moved,
-- but for the most part this table contains unprocessed messages.

CREATE TABLE [PeachtreeBus].[SampleQueue_Pending]
(
    [Id] BIGINT NOT NULL IDENTITY,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Priority] INT NOT NULL,
    [NotBefore] DATETIME2 NOT NULL,
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL,
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_SampleQueue_Pending_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[SampleQueue_Pending] ADD CONSTRAINT DF_SampleQueue_Pending_Retries DEFAULT((0)) FOR [Retries]
GO

CREATE INDEX IX_SampleQueue_Pending_GetNext ON [PeachtreeBus].[SampleQueue_Pending] ([Priority]) INCLUDE ([NotBefore])
GO

CREATE INDEX IX_SampleQueue_Pending_Enqueued ON [PeachtreeBus].[SampleQueue_Pending] ([Enqueued] DESC)
GO
