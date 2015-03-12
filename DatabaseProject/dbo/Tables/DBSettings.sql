CREATE TABLE [dbo].[DBSettings] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (8000) COLLATE Latin1_General_CI_AS NOT NULL,
    [Value]         VARCHAR (8000) COLLATE Latin1_General_CI_AS NOT NULL,
    [Comment]       VARCHAR (8000) COLLATE Latin1_General_CI_AS NOT NULL,
    [BeginDateTime] DATETIME       NOT NULL,
    [EndDateTime]   DATETIME       NOT NULL,
    CONSTRAINT [PK_DBSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_DBSettings]
    ON [dbo].[DBSettings]([Id] ASC);

