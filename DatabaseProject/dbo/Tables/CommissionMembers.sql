CREATE TABLE [dbo].[CommissionMembers] (
    [Id]                     INT      IDENTITY (1, 1) NOT NULL,
    [PersonStaffId]          INT      NULL,
    [StaffId]                INT      NULL,
    [CommissionMemberTypeId] INT      NOT NULL,
    [CommissionTypeId]       INT      NOT NULL,
    [BeginDateTime]          DATETIME NOT NULL,
    [EndDateTime]            DATETIME NOT NULL,
    CONSTRAINT [PK_CommissionMembers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionMembers_CommissionMemberTypes] FOREIGN KEY ([CommissionMemberTypeId]) REFERENCES [dbo].[CommissionMemberTypes] ([Id]),
    CONSTRAINT [FK_CommissionMembers_CommissionTypes] FOREIGN KEY ([CommissionTypeId]) REFERENCES [dbo].[CommissionTypes] ([Id]),
    CONSTRAINT [FK_CommissionMembers_PersonStaffs] FOREIGN KEY ([PersonStaffId]) REFERENCES [dbo].[PersonStaffs] ([Id]),
    CONSTRAINT [FK_CommissionMembers_Staffs] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[Staffs] ([Id])
);



