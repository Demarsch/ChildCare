CREATE TABLE [dbo].[FinancingSources] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (150) NOT NULL,
    [ShortName]     VARCHAR (50)  NOT NULL,
    [IsActive]      BIT           CONSTRAINT [DF__FinacingS__IsAct__7A672E12] DEFAULT ((1)) NOT NULL,
    [IsOrgContract] BIT           CONSTRAINT [DF_FinancingSources_IsOrgContract] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_FinacingSources] PRIMARY KEY CLUSTERED ([Id] ASC)
);



