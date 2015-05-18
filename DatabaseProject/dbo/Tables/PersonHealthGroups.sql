CREATE TABLE [dbo].[PersonHealthGroups] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT      NOT NULL,
    [HealthGroupId] INT      NOT NULL,
    [BeginDateTime] DATETIME NOT NULL,
    [EndDateTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_PersonHealthGroups] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonHealthGroups_HealthGroups] FOREIGN KEY ([HealthGroupId]) REFERENCES [dbo].[HealthGroups] ([Id]),
    CONSTRAINT [FK_PersonHealthGroups_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

