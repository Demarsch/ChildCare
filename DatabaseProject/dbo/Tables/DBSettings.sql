CREATE TABLE [dbo].[DBSettings] (
    [Id]    INT            IDENTITY (1, 1) NOT NULL,
    [Name]  VARCHAR (8000) NOT NULL,
    [Value] VARCHAR (8000) NOT NULL,
    CONSTRAINT [PK_DBSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);

