-- reference definition of a queue messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[QueueName_Pending]
(
    [Id] BIGINT CONSTRAINT PK_QueueName_Pending_Id PRIMARY KEY NOT NULL IDENTITY,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Priority] INT NOT NULL,
    [NotBefore] DATETIME2 NOT NULL,
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL,
    [Retries] TINYINT CONSTRAINT DF_QueueName_Pending_Retries DEFAULT ((0)) NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL
)
GO

CREATE INDEX IX_QueueName_Pending_GetNext ON [PeachtreeBus].[QueueName_Pending] ([Priority]) INCLUDE ([NotBefore])
GO

CREATE INDEX IX_QueueName_Pending_Enqueued ON [PeachtreeBus].[QueueName_Pending] ([Enqueued] DESC)
GO
