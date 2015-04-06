CREATE TABLE [dbo].[CommissionTypes] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (500) NOT NULL,
    [ShortName]     VARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    CONSTRAINT [PK_CommissionTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

