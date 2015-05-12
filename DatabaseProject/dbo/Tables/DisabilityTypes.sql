CREATE TABLE [dbo].[DisabilityTypes] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (255) NOT NULL,
    [BeginDate] DATETIME      NOT NULL,
    [EndDate]   DATETIME      NOT NULL,
    CONSTRAINT [PK_DisabilityTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

