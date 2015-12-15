CREATE TABLE [dbo].[IdentityDocumentTypes] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    VARCHAR (255) NOT NULL,
    [Options] VARCHAR (500) NULL,
    CONSTRAINT [PK_IdentityDocumentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);



