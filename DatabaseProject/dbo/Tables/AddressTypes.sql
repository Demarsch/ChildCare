CREATE TABLE [dbo].[AddressTypes] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Name]             VARCHAR (100) NOT NULL,
    [WithoutEndDate]   BIT           CONSTRAINT [DF_AddressTypes_WithoutEndDate] DEFAULT ((0)) NOT NULL,
    [PriorityForOKATO] INT           CONSTRAINT [DF_AddressTypes_PriorityForOKATO] DEFAULT ((0)) NOT NULL,
    [Category]         VARCHAR (500) CONSTRAINT [DF_AddressTypes_Category] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_AddressTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);







