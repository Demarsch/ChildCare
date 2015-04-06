CREATE TABLE [dbo].[CommissionProtocolMembers] (
    [Id]                   INT IDENTITY (1, 1) NOT NULL,
    [CommissionProtocolId] INT NOT NULL,
    [CommissionMemberId]   INT NOT NULL,
    [IsOfficial]           BIT NOT NULL,
    [IsAsked]              BIT NOT NULL,
    CONSTRAINT [PK_CommissionProtocolMembers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionProtocolMembers_CommissionMembers] FOREIGN KEY ([CommissionMemberId]) REFERENCES [dbo].[CommissionMembers] ([Id]),
    CONSTRAINT [FK_CommissionProtocolMembers_CommissionProtocols] FOREIGN KEY ([CommissionProtocolId]) REFERENCES [dbo].[CommissionProtocols] ([Id])
);

