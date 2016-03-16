CREATE TABLE [dbo].[IdentityDocumentTypes] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (255) NOT NULL,
    [Options]       VARCHAR (500) NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    [DocSeria]      VARCHAR (10)  CONSTRAINT [DF_IdentityDocumentTypes_DocSeria] DEFAULT ('') NOT NULL,
    [DocNumber]     VARCHAR (20)  CONSTRAINT [DF_IdentityDocumentTypes_DocNumber] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_IdentityDocumentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);





