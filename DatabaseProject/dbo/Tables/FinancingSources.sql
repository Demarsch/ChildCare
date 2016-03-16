CREATE TABLE [dbo].[FinancingSources] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]  INT           NULL,
    [Name]      VARCHAR (150) NOT NULL,
    [ShortName] VARCHAR (50)  NOT NULL,
    [IsActive]  BIT           CONSTRAINT [DF__FinacingS__IsAct__7A672E12] DEFAULT ((1)) NOT NULL,
    [Options]   VARCHAR (100) CONSTRAINT [DF_FinancingSources_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_FinacingSources] PRIMARY KEY CLUSTERED ([Id] ASC)
);









