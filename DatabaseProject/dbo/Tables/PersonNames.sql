﻿CREATE TABLE [dbo].[PersonNames] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT            NOT NULL,
    [LastName]      NVARCHAR (100) NOT NULL,
    [FirstName]     NVARCHAR (100) NOT NULL,
    [MiddleName]    NVARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME       NOT NULL,
    [EndDateTime]   DATETIME       NOT NULL,
    CONSTRAINT [PK_PersonNames] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonNames_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);





