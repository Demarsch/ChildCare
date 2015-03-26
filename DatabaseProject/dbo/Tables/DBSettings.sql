CREATE TABLE [dbo].[DBSettings] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (1000) NOT NULL,
    [Value]         VARCHAR (8000) NOT NULL,
    [DisplayName]   VARCHAR (1000) CONSTRAINT [DF_DBSettings_DisplayName] DEFAULT ('') NOT NULL,
    [Description]   VARCHAR (8000) CONSTRAINT [DF_DBSettings_Description] DEFAULT ('') NOT NULL,
    [Comment]       VARCHAR (8000) CONSTRAINT [DF_DBSettings_Comment] DEFAULT ('') NOT NULL,
    [BeginDateTime] DATETIME       NOT NULL,
    [EndDateTime]   DATETIME       NOT NULL,
    CONSTRAINT [PK_DBSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);










GO
CREATE NONCLUSTERED INDEX [IX_DBSettings]
    ON [dbo].[DBSettings]([Id] ASC);

