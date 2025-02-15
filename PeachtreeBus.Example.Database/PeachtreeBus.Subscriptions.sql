CREATE TABLE [PeachtreeBus].[Subscriptions]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[SubscriberId] UNIQUEIDENTIFIER NOT NULL,
	[Topic] NVARCHAR(128) NOT NULL,
	[ValidUntil] DATETIME2 NOT NULL,
	CONSTRAINT PK_Subscriptions_Id PRIMARY KEY ([Id]),
	CONSTRAINT AK_SubscriberTopic UNIQUE([SubscriberId], [Topic])
)
GO

CREATE INDEX IX_Subscriptions_SubscriberTopic ON [PeachtreeBus].[Subscriptions] ([SubscriberId], [Topic])
GO

CREATE INDEX IX_Subcriptions_ValidUntilTopic ON [PeachtreeBus].[Subscriptions] ([ValidUntil], [Topic])
GO
