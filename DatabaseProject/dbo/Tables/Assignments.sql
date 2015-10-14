CREATE TABLE [dbo].[Assignments] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [ParentId]          INT            NULL,
    [RecordTypeId]      INT            NOT NULL,
    [PersonId]          INT            NOT NULL,
    [AssignDateTime]    DATETIME       NOT NULL,
    [Duration]          INT            NOT NULL,
    [AssignUserId]      INT            NOT NULL,
    [AssignLpuId]       INT            NULL,
    [RoomId]            INT            NOT NULL,
    [FinancingSourceId] INT            NOT NULL,
    [CancelUserId]      INT            NULL,
    [CancelDateTime]    DATETIME       NULL,
    [Note]              VARCHAR (8000) CONSTRAINT [DF_Assignments_Note] DEFAULT ('') NOT NULL,
    [RecordId]          INT            NULL,
    [VisitId]           INT            NULL,
    [IsTemporary]       BIT            CONSTRAINT [DF__Assignmen__IsTem__4A8310C6] DEFAULT ((0)) NOT NULL,
    [CreationDateTime]  DATETIME       CONSTRAINT [DF__Assignmen__Creat__498EEC8D] DEFAULT (getdate()) NOT NULL,
    [BillingDateTime]   DATETIME       NULL,
    [Cost]              FLOAT (53)     CONSTRAINT [DF_Assignments_Cost] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Assignments_Assignments] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Assignments] ([Id]),
    CONSTRAINT [FK_Assignments_FinancingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinancingSources] ([Id]),
    CONSTRAINT [FK_Assignments_Orgs] FOREIGN KEY ([AssignLpuId]) REFERENCES [dbo].[Orgs] ([Id]),
    CONSTRAINT [FK_Assignments_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_Assignments_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_Assignments_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_Assignments_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]),
    CONSTRAINT [FK_Assignments_Users] FOREIGN KEY ([AssignUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Assignments_Users1] FOREIGN KEY ([CancelUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Assignments_Visits] FOREIGN KEY ([VisitId]) REFERENCES [dbo].[Visits] ([Id])
);


























GO



GO


