CREATE TABLE [dbo].[StaffCategories] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_StaffCategories] PRIMARY KEY CLUSTERED ([Id] ASC)
);

