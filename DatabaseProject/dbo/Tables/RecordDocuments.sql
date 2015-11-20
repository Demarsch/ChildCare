CREATE TABLE [dbo].[RecordDocuments] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [AssignmentId] INT NULL,
    [RecordId]     INT NULL,
    [DocumentId]   INT NOT NULL,
    CONSTRAINT [PK_RecordDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordDocuments_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments] ([Id]),
    CONSTRAINT [FK_RecordDocuments_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecordDocuments_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id])
);





