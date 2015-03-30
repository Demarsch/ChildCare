CREATE TABLE [dbo].[ChangeNameReasons] (
    [Id]                      INT           IDENTITY (1, 1) NOT NULL,
    [Name]                    VARCHAR (255) NOT NULL,
    [NeedCreateNewPersonName] BIT           CONSTRAINT [DF_ChangeNameReasons_NeedCreateNewPersonName] DEFAULT ((0)) NOT NULL,
    [BeginDateTime]           DATETIME      NOT NULL,
    [EndDateTime]             DATETIME      NOT NULL,
    CONSTRAINT [PK_ChangeNameReasons] PRIMARY KEY CLUSTERED ([Id] ASC)
);

