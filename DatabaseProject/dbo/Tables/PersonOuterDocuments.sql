CREATE TABLE [dbo].[PersonOuterDocuments] (
    [Id]                  INT IDENTITY (1, 1) NOT NULL,
    [PersonId]            INT NOT NULL,
    [OuterDocumentTypeId] INT NOT NULL,
    [DocumentId]          INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonOuterDocuments_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_PersonOuterDocuments_OuterDocumentTypes] FOREIGN KEY ([OuterDocumentTypeId]) REFERENCES [dbo].[OuterDocumentTypes] ([Id]),
    CONSTRAINT [FK_PersonOuterDocuments_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

