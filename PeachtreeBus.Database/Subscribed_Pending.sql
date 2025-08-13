-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[Subscribed_Pending]
(
    [Id] BIGINT CONSTRAINT PK_Subscribed_Pending_Id PRIMARY KEY NOT NULL IDENTITY,
    [SubscriberId] UNIQUEIDENTIFIER NOT NULL,
    [Topic] NVARCHAR(128) NOT NULL,
    [ValidUntil] DATETIME2 NOT NULL,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Priority] INT NOT NULL,
    [NotBefore] DATETIME2 NOT NULL,
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL,
    [Retries] TINYINT CONSTRAINT DF_Subscribed_Pending_Retries DEFAULT ((0)) NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL
)
GO

CREATE INDEX IX_Subscribed_Pending_NextMessage ON [PeachtreeBus].[Subscribed_Pending] ([SubscriberId], [Priority] DESC) INCLUDE ([NotBefore])
GO

CREATE INDEX IX_Subscribed_Pending_Enqueued ON [PeachtreeBus].[Subscribed_Pending] ([Enqueued])
GO
