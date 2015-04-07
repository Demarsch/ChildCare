CREATE TABLE [dbo].[CommissionSources] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [CommissionTypeId] INT           NOT NULL,
    [Name]             VARCHAR (500) NOT NULL,
    [ShortName]        VARCHAR (100) NOT NULL,
    [BeginDateTime]    DATETIME      NOT NULL,
    [EndDateTime]      DATETIME      NOT NULL,
    CONSTRAINT [PK_CommissionSources] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionSources_CommissionTypes] FOREIGN KEY ([CommissionTypeId]) REFERENCES [dbo].[CommissionTypes] ([Id])
);

