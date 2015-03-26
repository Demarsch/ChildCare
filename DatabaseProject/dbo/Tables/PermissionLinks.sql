CREATE TABLE [dbo].[PermissionLinks] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [ParentId] INT NOT NULL,
    [ChildId]  INT NOT NULL,
    CONSTRAINT [PK_PermissionLinks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PermissionLinks_Permissions] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Permissions] ([Id]),
    CONSTRAINT [FK_PermissionLinks_Permissions1] FOREIGN KEY ([ChildId]) REFERENCES [dbo].[Permissions] ([Id])
);

