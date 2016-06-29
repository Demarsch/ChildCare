CREATE TABLE [dbo].[Decisions] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [ParentId]       INT            NULL,
    [Name]           VARCHAR (2000) NOT NULL,
    [ShortName]      VARCHAR (1000) NOT NULL,
    [DecisionKindId] INT            NULL,
    [BeginDateTime]  DATETIME       NOT NULL,
    [EndDateTime]    DATETIME       NOT NULL,
    CONSTRAINT [PK_Decisions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Decisions_DecisionKinds] FOREIGN KEY ([DecisionKindId]) REFERENCES [dbo].[DecisionKinds] ([Id]),
    CONSTRAINT [FK_Decisions_Decisions] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Decisions] ([Id])
);











