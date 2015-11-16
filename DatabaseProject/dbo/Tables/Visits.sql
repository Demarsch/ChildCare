CREATE TABLE [dbo].[Visits] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [VisitTemplateId]   INT           NOT NULL,
    [PersonId]          INT           NOT NULL,
    [BeginDateTime]     DATETIME      NOT NULL,
    [EndDateTime]       DATETIME      NULL,
    [UrgentlyId]        INT           NOT NULL,
    [FinancingSourceId] INT           NOT NULL,
    [ContractId]        INT           NOT NULL,
    [SentLPUId]         INT           NOT NULL,
    [OKATO]             VARCHAR (14)  CONSTRAINT [DF_Visits_OKATO] DEFAULT ('') NOT NULL,
    [MKB]               VARCHAR (10)  CONSTRAINT [DF_Visits_MKB] DEFAULT ('') NOT NULL,
    [VisitResultId]     INT           NULL,
    [VisitOutcomeId]    INT           NULL,
    [Note]              VARCHAR (500) CONSTRAINT [DF_Visits_Note] DEFAULT ('') NOT NULL,
    [TotalCost]         FLOAT (53)    CONSTRAINT [DF_Visits_TotalCost] DEFAULT ((0)) NOT NULL,
    [IsCompleted]       BIT           NULL,
    [ExecutionPlaceId]  INT           NOT NULL,
    [RemovedByUserId]   INT           NULL,
    CONSTRAINT [PK_Visits] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Visits_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id]),
    CONSTRAINT [FK_Visits_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_Visits_Orgs] FOREIGN KEY ([SentLPUId]) REFERENCES [dbo].[Orgs] ([Id]),
    CONSTRAINT [FK_Visits_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_Visits_RecordContracts] FOREIGN KEY ([ContractId]) REFERENCES [dbo].[RecordContracts] ([Id]),
    CONSTRAINT [FK_Visits_Urgentlies] FOREIGN KEY ([UrgentlyId]) REFERENCES [dbo].[Urgentlies] ([Id]),
    CONSTRAINT [FK_Visits_Users_RemovedByUser] FOREIGN KEY ([RemovedByUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Visits_VisitOutcomes] FOREIGN KEY ([VisitOutcomeId]) REFERENCES [dbo].[VisitOutcomes] ([Id]),
    CONSTRAINT [FK_Visits_VisitResults] FOREIGN KEY ([VisitResultId]) REFERENCES [dbo].[VisitResults] ([Id]),
    CONSTRAINT [FK_Visits_VisitTemplates] FOREIGN KEY ([VisitTemplateId]) REFERENCES [dbo].[VisitTemplates] ([Id])
);













