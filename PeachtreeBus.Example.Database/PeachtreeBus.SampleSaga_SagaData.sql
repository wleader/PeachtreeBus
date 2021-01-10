-- A table to hold the SagaData for the SampleSaga.

CREATE TABLE [PeachtreeBus].[SampleSaga_SagaData]
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
	[SagaId] UNIQUEIDENTIFIER NOT NULL, 
	[Key] NVARCHAR(128) NOT NULL,
	[Data] NVARCHAR(MAX) NOT NULL,
	CONSTRAINT AK_SagaInstance UNIQUE([Key])
)
