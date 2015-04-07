CREATE TABLE [dbo].[RecordContracts] (
    [Id]                INT      IDENTITY (1, 1) NOT NULL,
    [FinancingSourceId] INT      NOT NULL,
    [BeginDateTime]     DATETIME NOT NULL,
    [EndDateTime]       DATETIME NOT NULL,
    CONSTRAINT [PK_RecordContracts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordContracts_FinacingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinacingSources] ([Id])
);

