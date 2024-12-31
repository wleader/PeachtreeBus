-- A table for failed messages from the queue named SampleQueue
CREATE TABLE [PeachtreeBus].[SampleQueue_Failed]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
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

ALTER TABLE [PeachtreeBus].[SampleQueue_Failed] ADD  CONSTRAINT DF_SampleQueue_Failed_Retries DEFAULT ((0)) FOR [Retries]
GO