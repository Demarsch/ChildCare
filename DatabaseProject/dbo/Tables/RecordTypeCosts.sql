CREATE TABLE [dbo].[RecordTypeCosts] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]      INT           NOT NULL,
    [FinancingSourceId] INT           NOT NULL,
    [BeginDate]         DATETIME      NOT NULL,
    [EndDate]           DATETIME      NOT NULL,
    [IsChild]           BIT           NULL,
    [IsIncome]          BIT           CONSTRAINT [DF_RecordTypeCosts_IsIncome] DEFAULT ((1)) NOT NULL,
    [AccountMaterials]  FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_AccountMaterials] DEFAULT ((0)) NOT NULL,
    [Amortisation]      FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_AccountMaterials1] DEFAULT ((0)) NOT NULL,
    [Salary]            FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_AccountMaterials2] DEFAULT ((0)) NOT NULL,
    [SoftInventory]     FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_Salary1] DEFAULT ((0)) NOT NULL,
    [Profitability]     FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_Profitability] DEFAULT ((1)) NOT NULL,
    [FullPrice]         FLOAT (53)    CONSTRAINT [DF_RecordTypeCosts_FullPrice] DEFAULT ((0)) NOT NULL,
    [InDateTime]        DATETIME      NOT NULL,
    [InUserLogin]       VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_RecordTypeCosts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypeCosts_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_RecordTypeCosts_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]) ON DELETE CASCADE
);







