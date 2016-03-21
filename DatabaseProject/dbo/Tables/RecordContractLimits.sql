CREATE TABLE [dbo].[RecordContractLimits] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [RecordContractId] INT NOT NULL,
    [RecordTypeId]     INT NOT NULL,
    [Count]            INT CONSTRAINT [DF_RecordContractLimits_Count] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RecordContractLimits] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordContractLimits_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecordContractLimits_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id])
);



