CREATE TABLE [dbo].[Records] (
    [Id]                 INT          IDENTITY (1, 1) NOT NULL,
    [PersonId]           INT          NOT NULL,
    [VisitId]            INT          NOT NULL,
    [ParentRecordId]     INT          NULL,
    [ParentAssignmentId] INT          NULL,
    [RoomId]             INT          NOT NULL,
    [RecordTypeId]       INT          NOT NULL,
    [RecordPeriodId]     INT          NOT NULL,
    [RecordContractId]   INT          NOT NULL,
    [ExecutionPlaceId]   INT          NOT NULL,
    [UrgentlyId]         INT          NOT NULL,
    [MKBId]              INT          NULL,
    [MKB]                VARCHAR (10) CONSTRAINT [DF_Records_MKB] DEFAULT ('') NOT NULL,
    [IsCompleted]        BIT          NOT NULL,
    [Number]             INT          CONSTRAINT [DF_Records_Number] DEFAULT ((0)) NOT NULL,
    [NumberType]         VARCHAR (10) CONSTRAINT [DF_Records_NumberType] DEFAULT ('') NOT NULL,
    [NumberYear]         INT          CONSTRAINT [DF_Records_NumberYear] DEFAULT ((0)) NOT NULL,
    [NumberTypeYear]     AS           (((ltrim(str([Number]))+[NumberType])+'-')+substring(ltrim(str([NumberYear])),(3),(2))),
    [BeginDateTime]      DATETIME     NOT NULL,
    [EndDateTime]        DATETIME     NULL,
    [ActualDateTime]     DATETIME     NOT NULL,
    [BillingDateTime]    DATETIME     NULL,
    [RemovedByUserId]    INT          NULL,
    CONSTRAINT [PK_Records] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Records_Assignments_ParentAssignment] FOREIGN KEY ([ParentAssignmentId]) REFERENCES [dbo].[Assignments] ([Id]),
    CONSTRAINT [FK_Records_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id]),
    CONSTRAINT [FK_Records_MKB] FOREIGN KEY ([MKBId]) REFERENCES [dbo].[MKB] ([Id]),
    CONSTRAINT [FK_Records_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_Records_RecordContracts] FOREIGN KEY ([RecordContractId]) REFERENCES [dbo].[RecordContracts] ([Id]),
    CONSTRAINT [FK_Records_RecordPeriods] FOREIGN KEY ([RecordPeriodId]) REFERENCES [dbo].[RecordPeriods] ([Id]),
    CONSTRAINT [FK_Records_Records] FOREIGN KEY ([ParentRecordId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_Records_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_Records_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]),
    CONSTRAINT [FK_Records_Urgentlies] FOREIGN KEY ([UrgentlyId]) REFERENCES [dbo].[Urgentlies] ([Id]),
    CONSTRAINT [FK_Records_Users] FOREIGN KEY ([RemovedByUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Records_Visits] FOREIGN KEY ([VisitId]) REFERENCES [dbo].[Visits] ([Id])
);

























