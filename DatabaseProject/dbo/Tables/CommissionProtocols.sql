CREATE TABLE [dbo].[CommissionProtocols] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]             INT            NOT NULL,
    [CommissionTypeId]     INT            NOT NULL,
    [DecisionId]           INT            NULL,
    [CommissionDate]       DATETIME       NOT NULL,
    [CommissionNumber]     INT            CONSTRAINT [DF_CommissionProtocols_CommissionNumber] DEFAULT ((0)) NOT NULL,
    [ProtocolNumber]       INT            CONSTRAINT [DF_CommissionProtocols_ProtocolNumber] DEFAULT ((0)) NOT NULL,
    [IsCompleted]          BIT            NULL,
    [IsExecuting]          BIT            NOT NULL,
    [IncomeDateTime]       DATETIME       NOT NULL,
    [BeginDateTime]        DATETIME       NULL,
    [CompleteDateTime]     DATETIME       NULL,
    [OutcomeDateTime]      DATETIME       NULL,
    [ToDoDateTime]         DATETIME       NULL,
    [Comment]              VARCHAR (8000) CONSTRAINT [DF_CommissionProtocols_Comment] DEFAULT ('') NOT NULL,
    [MKB]                  VARCHAR (10)   CONSTRAINT [DF_CommissionProtocols_MKB] DEFAULT ('') NOT NULL,
    [CommissionSourceId]   INT            NULL,
    [CommissionQuestionId] INT            NOT NULL,
    [PersonTalonId]        INT            NULL,
    [MedicalHelpTypeId]    INT            NULL,
    [RecordContractId]     INT            NULL,
    [PersonAddressId]      INT            NULL,
    [WaitingFor]           VARCHAR (500)  CONSTRAINT [DF_CommissionProtocols_WaitingFor] DEFAULT ('') NOT NULL,
    [Diagnos]              VARCHAR (8000) CONSTRAINT [DF_CommissionProtocols_Diagnos] DEFAULT ('') NOT NULL,
    [SentLPUId]            INT            NULL,
    [IsSended]             BIT            CONSTRAINT [DF_CommissionProtocols_IsSended] DEFAULT ((0)) NOT NULL,
    [InUserId]             INT            NOT NULL,
    [RemovedByUserId]      INT            NULL,
    CONSTRAINT [PK_CommissionProtocols] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionProtocols_CommissionQuestions] FOREIGN KEY ([CommissionQuestionId]) REFERENCES [dbo].[CommissionQuestions] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_CommissionSources] FOREIGN KEY ([CommissionSourceId]) REFERENCES [dbo].[CommissionSources] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_CommissionTypes] FOREIGN KEY ([CommissionTypeId]) REFERENCES [dbo].[CommissionTypes] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_CreateUsers] FOREIGN KEY ([InUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_Decisions] FOREIGN KEY ([DecisionId]) REFERENCES [dbo].[Decisions] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_MedicalHelpTypes] FOREIGN KEY ([MedicalHelpTypeId]) REFERENCES [dbo].[MedicalHelpTypes] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_Orgs] FOREIGN KEY ([SentLPUId]) REFERENCES [dbo].[Orgs] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_PersonAddresses] FOREIGN KEY ([PersonAddressId]) REFERENCES [dbo].[PersonAddresses] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_PersonTalons] FOREIGN KEY ([PersonTalonId]) REFERENCES [dbo].[PersonTalons] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id]),
    CONSTRAINT [FK_CommissionProtocols_RemovedUsers] FOREIGN KEY ([RemovedByUserId]) REFERENCES [dbo].[Users] ([Id])
);
































GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Черновик - в работе - завершено', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'IsCompleted';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Когда началась эта комиссия (первая отправка на рассмотрение)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'BeginDateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Когда нужно выполнить решение', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'ToDoDateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Отправка решения (документов)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'OutcomeDateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Видна участникам на расмотрении или нет (только когда - в работе)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'IsExecuting';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Когда принят запрос (документы) на комиссию', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'IncomeDateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Когда вынесено решение - момент перехода в завершенную', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'CommissionProtocols', @level2type = N'COLUMN', @level2name = N'CompleteDateTime';

