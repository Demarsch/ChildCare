CREATE TABLE [dbo].[Rooms] (
    [Id]     INT           IDENTITY (1, 1) NOT NULL,
    [Number] VARCHAR (10)  NOT NULL,
    [Name]   VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED ([Id] ASC)
);

