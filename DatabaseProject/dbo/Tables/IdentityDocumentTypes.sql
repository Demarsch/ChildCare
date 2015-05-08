CREATE TABLE [dbo].[IdentityDocumentTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_IdentityDocumentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

