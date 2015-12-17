CREATE TABLE [dbo].[RecordTypeUnits] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [RecordTypeId] INT NOT NULL,
    [UnitId]       INT NOT NULL,
    CONSTRAINT [PK_RecordTypeUnits] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypeUnits_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_RecordTypeUnits_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Units] ([Id])
);

