CREATE TABLE [dbo].[PersonMaritalStatuses] (
    [Id]              INT      IDENTITY (1, 1) NOT NULL,
    [PersonId]        INT      NOT NULL,
    [MaritalStatusId] INT      NOT NULL,
    [BeginDateTime]   DATETIME NOT NULL,
    [EndDateTime]     DATETIME NOT NULL,
    CONSTRAINT [PK_PersonMaritalStatuses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonMaritalStatuses_MaritalStatuses] FOREIGN KEY ([MaritalStatusId]) REFERENCES [dbo].[MaritalStatuses] ([Id]),
    CONSTRAINT [FK_PersonMaritalStatuses_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

