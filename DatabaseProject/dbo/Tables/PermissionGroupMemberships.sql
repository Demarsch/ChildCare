CREATE TABLE [dbo].[PermissionGroupMemberships] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [PermissionId] INT NOT NULL,
    [GroupId]      INT NOT NULL,
    CONSTRAINT [PK_PermissionGroupMemberships] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PermissionGroupMemberships_PermissionGroups] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[PermissionGroups] ([Id]),
    CONSTRAINT [FK_PermissionGroupMemberships_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id])
);

