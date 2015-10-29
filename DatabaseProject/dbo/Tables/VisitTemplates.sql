CREATE TABLE [dbo].[VisitTemplates] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [Name]              VARCHAR (500) NOT NULL,
    [ShortName]         VARCHAR (100) NOT NULL,
    [ContractId]        INT           NULL,
    [FinancingSourceId] INT           NULL,
    [ExecutionPlaceId]  INT           NOT NULL,
    [UrgentlyId]        INT           NULL,
    [BeginDateTime]     DATETIME      NOT NULL,
    [EndDateTime]       DATETIME      CONSTRAINT [DF_VisitTemplates_EndDateTime] DEFAULT ((3)) NOT NULL,
    CONSTRAINT [PK_VisitTemplates] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_VisitTemplates_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id]),
    CONSTRAINT [FK_VisitTemplates_ExecutionPlaces1] FOREIGN KEY ([UrgentlyId]) REFERENCES [dbo].[Urgentlies] ([Id]),
    CONSTRAINT [FK_VisitTemplates_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_VisitTemplates_RecordContracts] FOREIGN KEY ([ContractId]) REFERENCES [dbo].[RecordContracts] ([Id])
);



