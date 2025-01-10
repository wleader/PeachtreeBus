-- reference definition of a completed messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[QueueName_Completed]
(
    [Id] BIGINT NOT NULL,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Priority] INT NOT NULL,
    [NotBefore] DATETIME2 NOT NULL,
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL,
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_QueueName_Completed_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[QueueName_Completed] ADD CONSTRAINT DF_QueueName_Completed_Retries DEFAULT ((0)) FOR [Retries]
GO

CREATE INDEX IX_QueueName_Completed_Enqueued ON [PeachtreeBus].[QueueName_Completed] ([Enqueued] DESC)
GO