CREATE TABLE [dbo].[PersonTalons] (
    [Id]                INT          IDENTITY (1, 1) NOT NULL,
    [PersonId]          INT          NOT NULL,
    [TalonNumber]       VARCHAR (50) NOT NULL,
    [TalonDateTime]     DATETIME     NOT NULL,
    [MKB]               VARCHAR (10) NOT NULL,
    [RecordContractId]  INT          NOT NULL,
    [MedicalHelpTypeId] INT          NULL,
    [PersonAddressId]   INT          NULL,
    [IsCompleted]       BIT          NULL,
    CONSTRAINT [PK_PersonTalons] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonTalons_MedicalHelpTypes] FOREIGN KEY ([MedicalHelpTypeId]) REFERENCES [dbo].[MedicalHelpTypes] ([Id]),
    CONSTRAINT [FK_PersonTalons_PersonAddresses] FOREIGN KEY ([PersonAddressId]) REFERENCES [dbo].[PersonAddresses] ([Id]),
    CONSTRAINT [FK_PersonTalons_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonTalons_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id])
);







