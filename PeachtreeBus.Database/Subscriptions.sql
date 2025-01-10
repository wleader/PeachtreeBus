-- Reference definition for the Subscriptions table.

CREATE TABLE [PeachtreeBus].[Subscriptions]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[SubscriberId] UNIQUEIDENTIFIER NOT NULL,
	[Category] NVARCHAR(128) NOT NULL,
	[ValidUntil] DATETIME2 NOT NULL,
	CONSTRAINT PK_Subscriptions_Id PRIMARY KEY ([Id]),
	CONSTRAINT AK_SubscriberCategory UNIQUE([SubscriberId], [Category])
)
GO

CREATE INDEX IX_Subscriptions_SubscriberCategory ON [PeachtreeBus].[Subscriptions] ([SubscriberId], [Category])
GO

CREATE INDEX IX_Subcriptions_ValidUntilCategory ON [PeachtreeBus].[Subscriptions] ([ValidUntil], [Category])
GO
