CREATE TABLE [dbo].[RelativeRelationshipConnections] (
    [Id]                   INT IDENTITY (1, 1) NOT NULL,
    [ParentRelationshipId] INT NOT NULL,
    [ChildRelationshipId]  INT NOT NULL,
    CONSTRAINT [PK_RelativeRelationshipConnections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RelativeRelationshipConnections_RelativeRelationships] FOREIGN KEY ([ParentRelationshipId]) REFERENCES [dbo].[RelativeRelationships] ([Id]),
    CONSTRAINT [FK_RelativeRelationshipConnections_RelativeRelationships1] FOREIGN KEY ([ChildRelationshipId]) REFERENCES [dbo].[RelativeRelationships] ([Id])
);



