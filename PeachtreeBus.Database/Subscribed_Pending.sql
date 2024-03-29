﻿-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.

CREATE TABLE [PeachtreeBus].[Subscribed_Pending]
(
    [Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
    [SubscriberId] UNIQUEIDENTIFIER NOT NULL, 
    [ValidUntil] DATETIME2 NOT NULL,
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

ALTER TABLE [PeachtreeBus].[Subscribed_Pending] ADD DEFAULT ((0)) FOR [Retries]
GO