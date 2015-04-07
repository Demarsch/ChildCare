CREATE TABLE [dbo].[MedicalHelpTypes] (
    [Id]               INT      IDENTITY (1, 1) NOT NULL,
    [RecordContractId] INT      NULL,
    [BeginDateTime]    DATETIME NOT NULL,
    [EndDateTime]      DATETIME NOT NULL,
    CONSTRAINT [PK_MedicalHelpTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MedicalHelpTypes_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id])
);

