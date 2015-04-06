CREATE TABLE [dbo].[CommissionDecisions] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [CommissionProtocolId] INT            NOT NULL,
    [CommissionMemberId]   INT            NOT NULL,
    [DecisionId]           INT            NULL,
    [DecisionDate]         DATETIME       NULL,
    [IsOfficial]           BIT            NOT NULL,
    [IsAsked]              BIT            NOT NULL,
    [Comment]              VARCHAR (8000) NOT NULL,
    [DecisionInDateTime]   DATETIME       NULL,
    CONSTRAINT [PK_CommissionDecisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionDecisions_CommissionMembers] FOREIGN KEY ([CommissionMemberId]) REFERENCES [dbo].[CommissionMembers] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_CommissionProtocols] FOREIGN KEY ([CommissionProtocolId]) REFERENCES [dbo].[CommissionProtocols] ([Id]),
    CONSTRAINT [FK_CommissionDecisions_Decisions] FOREIGN KEY ([DecisionId]) REFERENCES [dbo].[Decisions] ([Id])
);

