CREATE TABLE [dbo].[DBSettings] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (8000) NOT NULL,
    [Value]         VARCHAR (8000) NOT NULL,
    [Comment]       VARCHAR (8000) NOT NULL,
    [BeginDateTime] DATETIME       NOT NULL,
    [EndDateTime]   DATETIME       NULL,
    CONSTRAINT [PK_DBSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_DBSettings]
    ON [dbo].[DBSettings]([Id] ASC);

