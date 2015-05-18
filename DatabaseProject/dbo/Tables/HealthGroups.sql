CREATE TABLE [dbo].[HealthGroups] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (255) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    CONSTRAINT [PK_HealthGroups] PRIMARY KEY CLUSTERED ([Id] ASC)
);

