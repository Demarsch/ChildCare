CREATE TABLE [dbo].[CommissionFilters] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    VARCHAR (100) CONSTRAINT [DF_CommissionFilters_Name] DEFAULT ('') NOT NULL,
    [Options] VARCHAR (500) CONSTRAINT [DF_CommissionFilters_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_CommissionFilters] PRIMARY KEY CLUSTERED ([Id] ASC)
);

