CREATE TABLE [dbo].[CommissionProtocols] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]             INT            NOT NULL,
    [CommissionTypeId]     INT            NOT NULL,
    [DecisionId]           INT            NULL,
    [DocumentNumber]       INT            NULL,
    [DocumentDate]         DATETIME       NULL,
    [IsCompleted]          BIT            NULL,
    [BeginDateTime]        DATETIME       NOT NULL,
    [Comment]              VARCHAR (8000) NOT NULL,
    [MKB]                  VARCHAR (10)   NOT NULL,
    [InUserId]             INT            NULL,
    [DecisionDate]         DATETIME       NULL,
    [CommissionSourceId]   INT            NOT NULL,
    [CommissionQuestionId] INT            NOT NULL,
    [PersonTalonId]        INT            NULL,
    [MedicalHelpTypeId]    INT            NULL,
    [RecordContractId]     INT            NULL,
    CONSTRAINT [PK_CommissionProtocols] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionProtocols_CommissionSources] FOREIGN KEY ([CommissionSourceId]) REFERENCES [dbo].[CommissionSources] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_MedicalHelpTypes] FOREIGN KEY ([MedicalHelpTypeId]) REFERENCES [dbo].[MedicalHelpTypes] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_PersonTalons] FOREIGN KEY ([PersonTalonId]) REFERENCES [dbo].[PersonTalons] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_Users] FOREIGN KEY ([InUserId]) REFERENCES [dbo].[Users] ([Id])
);





