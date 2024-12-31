﻿-- reference definition of a saga data table. A specifically named table
-- will need to exist for each saga defined in code.

CREATE TABLE [PeachtreeBus].[SagaName_SagaData]
(
	[Id] BIGINT NOT NULL IDENTITY,
	[SagaId] UNIQUEIDENTIFIER NOT NULL, 
	[Key] NVARCHAR(128) NOT NULL,
	[Data] NVARCHAR(MAX) NOT NULL,
	CONSTRAINT PK_SagaName_SagaData_Id PRIMARY KEY ([Id]),
	CONSTRAINT AK_SagaInstance UNIQUE([Key])
)
