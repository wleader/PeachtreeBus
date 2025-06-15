-- A table for failed messages from the queue named SampleQueue
CREATE TABLE [PeachtreeBus].[SampleQueue_Failed]
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
    CONSTRAINT PK_SampleQueue_Failed_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[SampleQueue_Failed] ADD  CONSTRAINT DF_SampleQueue_Failed_Retries DEFAULT ((0)) FOR [Retries]
GO

CREATE INDEX IX_SampleQueue_Failed_Enqueued ON [PeachtreeBus].[SampleQueue_Failed] ([Enqueued] DESC)
GO
