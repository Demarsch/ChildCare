CREATE TABLE [dbo].[PersonDisabilities] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [PersonId]         INT           NOT NULL,
    [DisabilityTypeId] INT           NOT NULL,
    [Series]           VARCHAR (10)  CONSTRAINT [DF_PersonDisabilities_Series] DEFAULT ('') NOT NULL,
    [Number]           VARCHAR (50)  CONSTRAINT [DF_PersonDisabilities_Number] DEFAULT ('') NOT NULL,
    [GivenOrg]         VARCHAR (500) CONSTRAINT [DF_PersonDisabilities_GivenOrg] DEFAULT ('') NOT NULL,
    [BeginDate]        DATETIME      NOT NULL,
    [EndDate]          DATETIME      NOT NULL,
    CONSTRAINT [PK_PersonDisabilities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonDisabilities_PersonDisabilities] FOREIGN KEY ([DisabilityTypeId]) REFERENCES [dbo].[PersonDisabilities] ([Id]),
    CONSTRAINT [FK_PersonDisabilities_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);



