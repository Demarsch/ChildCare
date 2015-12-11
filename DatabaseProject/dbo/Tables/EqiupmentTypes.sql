CREATE TABLE [dbo].[EqiupmentTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (500) NOT NULL,
    [Code] INT           NOT NULL,
    CONSTRAINT [PK_EqiupmentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

