CREATE TABLE [dbo].[RecordDocuments] (
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    [RecordId]   INT NOT NULL,
    [DocumentId] INT NOT NULL,
    CONSTRAINT [PK_RecordDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordDocuments_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_RecordDocuments_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id])
);

