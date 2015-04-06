CREATE TABLE [dbo].[Branches] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]      INT           NULL,
    [Name]          VARCHAR (500) NOT NULL,
    [ShortName]     VARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Branches_Branches] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Branches] ([Id])
);

