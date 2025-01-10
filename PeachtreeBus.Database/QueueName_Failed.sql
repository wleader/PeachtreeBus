-- reference definition of a error messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[QueueName_Failed]
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
    CONSTRAINT PK_QueueName_Failed_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[QueueName_Failed] ADD CONSTRAINT DF_QueueName_Failed_Retries DEFAULT ((0)) FOR [Retries]
GO

CREATE INDEX IX_QueueName_Failed_Enqueued ON [PeachtreeBus].[QueueName_Failed] ([Enqueued] DESC)
GO