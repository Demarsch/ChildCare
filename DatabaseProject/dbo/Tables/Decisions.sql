CREATE TABLE [dbo].[Decisions] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [ParentId]        INT            NULL,
    [Name]            VARCHAR (2000) NOT NULL,
    [ShortName]       VARCHAR (1000) NOT NULL,
    [BeginDateTime]   DATETIME       NOT NULL,
    [EndDateTime]     DATETIME       NOT NULL,
    [ColorSettingsId] INT            NULL,
    CONSTRAINT [PK_Decisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Decisions_ColorsSettings] FOREIGN KEY ([ColorSettingsId]) REFERENCES [dbo].[ColorsSettings] ([Id]),
    CONSTRAINT [FK_Decisions_Decisions] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Decisions] ([Id])
);









