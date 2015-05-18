CREATE TABLE [dbo].[PersonNationalities] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT      NOT NULL,
    [CountryId]     INT      NOT NULL,
    [BeginDateTime] DATETIME NOT NULL,
    [EndDateTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_PersonNationalities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonNationalities_Countries] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Countries] ([Id]),
    CONSTRAINT [FK_PersonNationalities_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

