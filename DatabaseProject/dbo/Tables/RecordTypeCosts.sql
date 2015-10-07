CREATE TABLE [dbo].[RecordTypeCosts] (
    [Id]                INT        IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]      INT        NOT NULL,
    [FinancingSourceId] INT        NOT NULL,
    [BeginDate]         DATETIME   NOT NULL,
    [EndDate]           DATETIME   NOT NULL,
    [FullPrice]         FLOAT (53) NULL,
    CONSTRAINT [PK_RecordTypeCosts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypeCosts_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_RecordTypeCosts_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id])
);

