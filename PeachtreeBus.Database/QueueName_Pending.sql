-- reference definition of a queue messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[QueueName_Pending]
(
	[Id] BIGINT NOT NULL IDENTITY, 
    [MessageId] UNIQUEIDENTIFIER NOT NULL, 
    [NotBefore] DATETIME2 NOT NULL, 
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL, 
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_QueueName_Pending_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[QueueName_Pending] ADD CONSTRAINT DF_QueueName_Pending_Retries DEFAULT ((0)) FOR [Retries]
GO