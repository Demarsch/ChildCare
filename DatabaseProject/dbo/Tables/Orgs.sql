CREATE TABLE [dbo].[Orgs] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (500) NOT NULL,
    CONSTRAINT [PK_Orgs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

