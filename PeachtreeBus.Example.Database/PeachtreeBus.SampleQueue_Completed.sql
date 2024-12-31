-- A table for completed messages from the queue named SampleQueue
CREATE TABLE [PeachtreeBus].[SampleQueue_Completed]
(
	[Id] BIGINT NOT NULL, 
    [MessageId] UNIQUEIDENTIFIER NOT NULL, 
    [NotBefore] DATETIME2 NOT NULL, 
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL, 
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_SampleQueue_Completed_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[SampleQueue_Completed] ADD CONSTRAINT DF_SampleQueue_Completed_Retries DEFAULT ((0)) FOR [Retries]
GO