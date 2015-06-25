CREATE TABLE [dbo].[PersonEducations] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT      NOT NULL,
    [EducationId]   INT      NOT NULL,
    [BeginDateTime] DATETIME NOT NULL,
    [EndDateTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_PersonEducations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonEducations_Educations] FOREIGN KEY ([EducationId]) REFERENCES [dbo].[Educations] ([Id]),
    CONSTRAINT [FK_PersonEducations_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);



