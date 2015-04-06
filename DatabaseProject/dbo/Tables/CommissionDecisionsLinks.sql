CREATE TABLE [dbo].[CommissionDecisionsLinks] (
    [Id]                     INT      IDENTITY (1, 1) NOT NULL,
    [CommissionTypeId]       INT      NOT NULL,
    [CommissionTypeMemberId] INT      NOT NULL,
    [DecisionId]             INT      NOT NULL,
    [BeginDateTime]          DATETIME NOT NULL,
    [EndDateTime]            DATETIME NOT NULL,
    CONSTRAINT [PK_CommissionDecisionsLinks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionDecisionsLinks_CommissionMemberTypes] FOREIGN KEY ([CommissionTypeMemberId]) REFERENCES [dbo].[CommissionMemberTypes] ([Id]),
    CONSTRAINT [FK_CommissionDecisionsLinks_CommissionTypes] FOREIGN KEY ([CommissionTypeId]) REFERENCES [dbo].[CommissionTypes] ([Id]),
    CONSTRAINT [FK_CommissionDecisionsLinks_Decisions] FOREIGN KEY ([DecisionId]) REFERENCES [dbo].[Decisions] ([Id])
);

