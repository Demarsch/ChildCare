CREATE TABLE [dbo].[CommissionTypeGroups] (
    [Id]            INT           NOT NULL,
    [Name]          VARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    CONSTRAINT [PK_CommissionTypeGroups] PRIMARY KEY CLUSTERED ([Id] ASC)
);

