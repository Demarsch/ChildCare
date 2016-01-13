CREATE TABLE [dbo].[PermissionGroups] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100)  NOT NULL,
    [Description] VARCHAR (1000) NULL,
    CONSTRAINT [PK_PermissionGroups] PRIMARY KEY CLUSTERED ([Id] ASC)
);



