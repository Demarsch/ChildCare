CREATE TABLE [dbo].[Countries] (
    [Id]                   INT           IDENTITY (1, 1) NOT NULL,
    [Name]                 VARCHAR (255) NOT NULL,
    [IsDefaultNationality] BIT           CONSTRAINT [DF_Countries_IsDefaultNationality] DEFAULT ((0)) NOT NULL,
    [BeginDateTime]        DATETIME      NOT NULL,
    [EndDateTime]          DATETIME      NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([Id] ASC)
);



