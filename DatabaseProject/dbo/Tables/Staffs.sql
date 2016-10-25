CREATE TABLE [dbo].[Staffs] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]   INT           NULL,
    [Name]       VARCHAR (500) NOT NULL,
    [ShortName]  VARCHAR (100) NOT NULL,
    [CategoryId] INT           NOT NULL,
    [IsSenior]   BIT           NOT NULL,
    [Options]    VARCHAR (50)  CONSTRAINT [DF_Staffs_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Staffs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Staffs_StaffCategories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[StaffCategories] ([Id])
);





