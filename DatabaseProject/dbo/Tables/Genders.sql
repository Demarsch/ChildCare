CREATE TABLE [dbo].[Genders] (
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (50) NOT NULL,
    [ShortName] VARCHAR (10) CONSTRAINT [DF_Genders_ShortName] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Genders] PRIMARY KEY CLUSTERED ([Id] ASC)
);



