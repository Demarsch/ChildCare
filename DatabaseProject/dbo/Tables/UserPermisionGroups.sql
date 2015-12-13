CREATE TABLE [dbo].[UserPermisionGroups] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [UserId]            INT NOT NULL,
    [PermissionGroupId] INT NOT NULL,
    CONSTRAINT [PK_UserPermisionGroups] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserPermisionGroups_PermissionGroups] FOREIGN KEY ([PermissionGroupId]) REFERENCES [dbo].[PermissionGroups] ([Id]),
    CONSTRAINT [FK_UserPermisionGroups_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);

