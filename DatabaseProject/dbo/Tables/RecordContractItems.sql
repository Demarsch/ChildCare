CREATE TABLE [dbo].[RecordContractItems] (
    [Id]               INT        IDENTITY (1, 1) NOT NULL,
    [RecordContractId] INT        NOT NULL,
    [AssignmentId]     INT        NULL,
    [RecordTypeId]     INT        NOT NULL,
    [Count]            INT        CONSTRAINT [DF_RecordContractItems_Count] DEFAULT ((1)) NOT NULL,
    [Cost]             FLOAT (53) CONSTRAINT [DF_RecordContractItems_Cost] DEFAULT ((0)) NOT NULL,
    [IsPaid]           BIT        CONSTRAINT [DF_RecordContractItems_IsPaid] DEFAULT ((0)) NOT NULL,
    [PaymentTypeId]    INT        NOT NULL,
    [Appendix]         INT        NULL,
    [InUserId]         INT        NOT NULL,
    [InDateTime]       DATETIME   NOT NULL,
    CONSTRAINT [PK_RecordContractItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordContractItems_Assignments] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments] ([Id]),
    CONSTRAINT [FK_RecordContractItems_PaymentTypes] FOREIGN KEY ([PaymentTypeId]) REFERENCES [dbo].[PaymentTypes] ([Id]),
    CONSTRAINT [FK_RecordContractItems_PersonStaffs] FOREIGN KEY ([InUserId]) REFERENCES [dbo].[PersonStaffs] ([Id]),
    CONSTRAINT [FK_RecordContractItems_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id]),
    CONSTRAINT [FK_RecordContractItems_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id])
);

