CREATE TABLE [dbo].[PersonIdentityDocuments] (
    [Id]                     INT           IDENTITY (1, 1) NOT NULL,
    [PersonId]               INT           NOT NULL,
    [IdentityDocumentTypeId] INT           NOT NULL,
    [Series]                 VARCHAR (10)  CONSTRAINT [DF_PersonIdentityDocuments_Series] DEFAULT ('') NOT NULL,
    [Number]                 VARCHAR (50)  CONSTRAINT [DF_PersonIdentityDocuments_Number] DEFAULT ('') NOT NULL,
    [GivenOrg]               VARCHAR (500) CONSTRAINT [DF_PersonIdentityDocuments_GivenOrg] DEFAULT ('') NOT NULL,
    [BeginDate]              DATETIME      NOT NULL,
    [EndDate]                DATETIME      NOT NULL,
    CONSTRAINT [PK_PersonIdentityDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonIdentityDocuments_IdentityDocumentTypes] FOREIGN KEY ([IdentityDocumentTypeId]) REFERENCES [dbo].[IdentityDocumentTypes] ([Id]),
    CONSTRAINT [FK_PersonIdentityDocuments_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

