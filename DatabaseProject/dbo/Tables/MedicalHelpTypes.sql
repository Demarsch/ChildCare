CREATE TABLE [dbo].[MedicalHelpTypes] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [RecordContractId] INT           NULL,
    [BeginDateTime]    DATETIME      NOT NULL,
    [EndDateTime]      DATETIME      NOT NULL,
    [Code]             VARCHAR (20)  NOT NULL,
    [Name]             VARCHAR (500) CONSTRAINT [DF_MedicalHelpTypes_Name] DEFAULT ('') NOT NULL,
    [ShortName]        VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_MedicalHelpTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MedicalHelpTypes_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id])
);



