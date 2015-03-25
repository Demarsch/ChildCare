CREATE TABLE [dbo].[InsuranceDocumentTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_InsuranceDocumentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

