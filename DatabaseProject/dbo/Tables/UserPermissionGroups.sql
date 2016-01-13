CREATE TABLE [dbo].[UserPermissionGroups] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [UserId]            INT NOT NULL,
    [PermissionGroupId] INT NOT NULL,
    CONSTRAINT [PK_UserPermissionGroups] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserPermissionGroups_PermissionGroups] FOREIGN KEY ([PermissionGroupId]) REFERENCES [dbo].[PermissionGroups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserPermissionGroups_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);

