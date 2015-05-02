CREATE TABLE [dbo].[Assignments] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]      INT            NOT NULL,
    [PersonId]          INT            NOT NULL,
    [AssignDateTime]    DATETIME       NOT NULL,
    [AssignUserId]      INT            NOT NULL,
    [RoomId]            INT            NOT NULL,
    [FinancingSourceId] INT            NOT NULL,
    [CancelUserId]      INT            NULL,
    [CancelReasonId]    INT            NULL,
    [Note]              VARCHAR (8000) CONSTRAINT [DF_Assignments_Note] DEFAULT ('') NOT NULL,
    [RecordId]          INT            NULL,
    [IsTemporary]       BIT            DEFAULT ((0)) NOT NULL,
    [CreationDateTime]  DATETIME       DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Assignments_FinacingSources] FOREIGN KEY ([FinancingSourceId]) REFERENCES [dbo].[FinacingSources] ([Id]),
    CONSTRAINT [FK_Assignments_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_Assignments_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_Assignments_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_Assignments_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]),
    CONSTRAINT [FK_Assignments_Users] FOREIGN KEY ([AssignUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Assignments_Users1] FOREIGN KEY ([CancelUserId]) REFERENCES [dbo].[Users] ([Id])
);








GO



GO


