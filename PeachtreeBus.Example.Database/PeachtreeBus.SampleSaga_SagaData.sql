-- A table to hold the SagaData for the SampleSaga.

CREATE TABLE [PeachtreeBus].[SampleSaga_SagaData]
(
    [Id] BIGINT NOT NULL IDENTITY,
    [SagaId] UNIQUEIDENTIFIER NOT NULL, 
    [Key] NVARCHAR(128) NOT NULL,
    [Data] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_SampleSaga_SagaData_Id PRIMARY KEY ([Id])
)
GO

CREATE UNIQUE INDEX IX_SampleSaga_SagaData_Key ON [PeachtreeBus].[SampleSaga_SagaData] ([Key])
GO
