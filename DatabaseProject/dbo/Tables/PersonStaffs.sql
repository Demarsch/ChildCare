CREATE TABLE [dbo].[PersonStaffs] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT      NOT NULL,
    [BranchId]      INT      NOT NULL,
    [StaffId]       INT      NOT NULL,
    [BeginDateTime] DATETIME NOT NULL,
    [EndDateTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_PersonStaffs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonStaffs_Branches] FOREIGN KEY ([BranchId]) REFERENCES [dbo].[Branches] ([Id]),
    CONSTRAINT [FK_PersonStaffs_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonStaffs_Staffs] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[Staffs] ([Id])
);



