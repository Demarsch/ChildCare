CREATE TABLE [dbo].[StaffCategories] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [ParentId] INT           NULL,
    [Name]     VARCHAR (100) NOT NULL,
    [Options]  VARCHAR (50)  CONSTRAINT [DF_StaffCategories_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_StaffCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StaffCategories_StaffCategories1] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[StaffCategories] ([Id])
);



