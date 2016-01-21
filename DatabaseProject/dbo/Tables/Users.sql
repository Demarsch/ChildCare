CREATE TABLE [dbo].[Users] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT           NOT NULL,
    [SID]           VARCHAR (255) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    [Login]         VARCHAR (200) CONSTRAINT [DF_Users_Login] DEFAULT ('') NOT NULL,
    [SystemName]    VARCHAR (300) CONSTRAINT [DF_Users_SystemName] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Users_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);
















GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'идентификатор пользователя в SQLServer', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Users', @level2type = N'COLUMN', @level2name = N'SID';

