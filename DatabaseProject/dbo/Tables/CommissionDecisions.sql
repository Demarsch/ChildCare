CREATE TABLE [dbo].[CommissionDecisions] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [CommissionProtocolId] INT            NOT NULL,
    [CommissionMemberId]   INT            NOT NULL,
    [IsOfficial]           BIT            NOT NULL,
    [DecisionId]           INT            NULL,
    [ToDoDateTime]         DATETIME       NULL,
    [Comment]              VARCHAR (8000) NOT NULL,
    [CommissionStage]      INT            CONSTRAINT [DF_CommissionDecisions_CommissionStage] DEFAULT ((0)) NOT NULL,
    [InitiatorMemberId]    INT            NOT NULL,
    [DecisionDateTime]     DATETIME       NULL,
    [RemovedByUserId]      INT            NULL,
    CONSTRAINT [PK_CommissionDecisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionDecisions_CommissionMembers] FOREIGN KEY ([CommissionMemberId]) REFERENCES [dbo].[CommissionMembers] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_CommissionMembers1] FOREIGN KEY ([InitiatorMemberId]) REFERENCES [dbo].[CommissionMembers] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_CommissionProtocols] FOREIGN KEY ([CommissionProtocolId]) REFERENCES [dbo].[CommissionProtocols] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_Decisions] FOREIGN KEY ([DecisionId]) REFERENCES [dbo].[Decisions] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_Users] FOREIGN KEY ([RemovedByUserId]) REFERENCES [dbo].[Users] ([Id])
);





