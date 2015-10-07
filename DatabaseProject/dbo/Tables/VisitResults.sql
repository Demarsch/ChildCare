CREATE TABLE [dbo].[VisitResults] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Code]             INT           NOT NULL,
    [ExecutionPlaceId] INT           NOT NULL,
    [Name]             VARCHAR (255) NOT NULL,
    [BeginDateTime]    DATETIME      NOT NULL,
    [EndDateTime]      DATETIME      NOT NULL,
    CONSTRAINT [PK_VisitResults] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_VisitResults_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id])
);

