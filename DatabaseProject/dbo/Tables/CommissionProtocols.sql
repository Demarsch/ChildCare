CREATE TABLE [dbo].[CommissionProtocols] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [PersonId]         INT            NOT NULL,
    [CommissionTypeId] INT            NOT NULL,
    [DecisionId]       INT            NULL,
    [DocumentNumber]   INT            NULL,
    [DocumentDate]     DATETIME       NULL,
    [IsCompleted]      BIT            NULL,
    [BeginDateTime]    DATETIME       NOT NULL,
    [Comment]          VARCHAR (8000) NOT NULL,
    CONSTRAINT [PK_CommissionProtocols] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionProtocols_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);

