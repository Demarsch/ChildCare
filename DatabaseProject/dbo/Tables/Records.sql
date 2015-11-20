CREATE TABLE [dbo].[Records] (
    [Id]              INT          IDENTITY (1, 1) NOT NULL,
    [PersonId]        INT          NOT NULL,
    [ParentId]        INT          NULL,
    [VisitId]         INT          NOT NULL,
    [RoomId]          INT          NOT NULL,
    [RecordTypeId]    INT          NOT NULL,
    [RecordPeriodId]  INT          NOT NULL,
    [UrgentlyId]      INT          NOT NULL,
    [BranchId]        INT          NOT NULL,
    [Number]          VARCHAR (50) CONSTRAINT [DF_Records_Number] DEFAULT ('') NOT NULL,
    [IsCompleted]     BIT          NOT NULL,
    [MKB]             VARCHAR (10) CONSTRAINT [DF_Records_MKB] DEFAULT ('') NOT NULL,
    [BeginDateTime]   DATETIME     NOT NULL,
    [EndDateTime]     DATETIME     NOT NULL,
    [ActualDateTime]  DATETIME     NOT NULL,
    [OKATO]           VARCHAR (16) CONSTRAINT [DF_Records_OKATO] DEFAULT ('') NOT NULL,
    [BillingDateTime] DATETIME     NULL,
    [RemovedByUserId] INT          NULL,
    CONSTRAINT [PK_Records] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Records_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_Records_RecordPeriods] FOREIGN KEY ([RecordPeriodId]) REFERENCES [dbo].[RecordPeriods] ([Id]),
    CONSTRAINT [FK_Records_Records] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_Records_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_Records_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]),
    CONSTRAINT [FK_Records_Urgentlies] FOREIGN KEY ([UrgentlyId]) REFERENCES [dbo].[Urgentlies] ([Id]),
    CONSTRAINT [FK_Records_Users] FOREIGN KEY ([RemovedByUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Records_Visits] FOREIGN KEY ([VisitId]) REFERENCES [dbo].[Visits] ([Id])
);











