CREATE TABLE [dbo].[RecordContracts] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [Number]            INT            NULL,
    [ContractName]      VARCHAR (1000) NOT NULL,
    [BeginDateTime]     DATETIME       NOT NULL,
    [EndDateTime]       DATETIME       NOT NULL,
    [FinancingSourceId] INT            NOT NULL,
    [ClientId]          INT            NULL,
    [ConsumerId]        INT            NULL,
    [OrgId]             INT            NULL,
    [ContractCost]      FLOAT (53)     CONSTRAINT [DF_RecordContracts_ContractCost] DEFAULT ((0.0)) NOT NULL,
    [PaymentTypeId]     INT            NOT NULL,
    [TransactionNumber] VARCHAR (50)   CONSTRAINT [DF_RecordContracts_TransactionNumber] DEFAULT ('') NOT NULL,
    [TransactionDate]   VARCHAR (50)   NOT NULL,
    [Priority]          INT            NOT NULL,
    [Options]           VARCHAR (1000) CONSTRAINT [DF_RecordContracts_Options] DEFAULT ('') NOT NULL,
    [InDateTime]        DATETIME       NOT NULL,
    [InUserId]          INT            NOT NULL,
    CONSTRAINT [PK_RecordContracts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordContracts_Clients] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_RecordContracts_Consumers] FOREIGN KEY ([ConsumerId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_RecordContracts_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_RecordContracts_Orgs] FOREIGN KEY ([OrgId]) REFERENCES [dbo].[Orgs] ([Id]),
    CONSTRAINT [FK_RecordContracts_PaymentTypes] FOREIGN KEY ([PaymentTypeId]) REFERENCES [dbo].[PaymentTypes] ([Id]),
    CONSTRAINT [FK_RecordContracts_PersonStaffs] FOREIGN KEY ([InUserId]) REFERENCES [dbo].[PersonStaffs] ([Id])
);





