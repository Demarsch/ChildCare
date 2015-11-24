CREATE TABLE [dbo].[RecordMembers] (
    [Id]                         INT IDENTITY (1, 1) NOT NULL,
    [RecordId]                   INT NOT NULL,
    [PersonStaffId]              INT NOT NULL,
    [RecordTypeRolePermissionId] INT NOT NULL,
    [IsActive]                   BIT CONSTRAINT [DF_RecordMembers_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_RecordMembers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordMembers_PersonStaffs] FOREIGN KEY ([PersonStaffId]) REFERENCES [dbo].[PersonStaffs] ([Id]),
    CONSTRAINT [FK_RecordMembers_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_RecordMembers_RecordTypeRolePermissions] FOREIGN KEY ([RecordTypeRolePermissionId]) REFERENCES [dbo].[RecordTypeRolePermissions] ([Id])
);



