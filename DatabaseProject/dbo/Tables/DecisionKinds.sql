CREATE TABLE [dbo].[DecisionKinds] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [Name]            VARCHAR (100) CONSTRAINT [DF_DecisionKinds_Name] DEFAULT ('') NOT NULL,
    [Type]            BIT           NULL,
    [ColorSettingsId] INT           NOT NULL,
    [BeginDateTime]   DATETIME      NOT NULL,
    [EndDateTime]     DATETIME      NOT NULL,
    CONSTRAINT [PK_DecisionKinds] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DecisionKinds_ColorsSettings] FOREIGN KEY ([ColorSettingsId]) REFERENCES [dbo].[ColorsSettings] ([Id])
);



