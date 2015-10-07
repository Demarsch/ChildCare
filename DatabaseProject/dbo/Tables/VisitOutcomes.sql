CREATE TABLE [dbo].[VisitOutcomes] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Code]             INT           NOT NULL,
    [ExecutionPlaceId] INT           NOT NULL,
    [Name]             VARCHAR (100) NOT NULL,
    [IsDefault]        BIT           CONSTRAINT [DF_VisitOutcomes_IsDefault] DEFAULT ((0)) NOT NULL,
    [BeginDateTime]    DATETIME      NOT NULL,
    [EndDateTime]      DATETIME      NOT NULL,
    CONSTRAINT [PK_VisitOutcomes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_VisitOutcomes_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id])
);

