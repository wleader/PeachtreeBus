-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[Subscribed_Failed]
(
    [Id] BIGINT NOT NULL,
    [SubscriberId] UNIQUEIDENTIFIER NOT NULL,
    [Topic] NVARCHAR(128) NOT NULL,
    [ValidUntil] DATETIME2 NOT NULL,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Priority] INT NOT NULL,
    [NotBefore] DATETIME2 NOT NULL,
    [Enqueued] DATETIME2 NOT NULL,
    [Completed] DATETIME2 NULL,
    [Failed] DATETIME2 NULL,
    [Retries] TINYINT NOT NULL,
    [Headers] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_Subscribed_Failed_Id PRIMARY KEY ([Id])
)
GO

ALTER TABLE [PeachtreeBus].[Subscribed_Failed] ADD CONSTRAINT DF_Subscribed_Failed_Retries DEFAULT ((0)) FOR [Retries]
GO

CREATE INDEX IX_Subscribed_Failed_Enqueued ON [PeachtreeBus].[Subscribed_Failed] ([Enqueued])
GO
