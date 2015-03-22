CREATE TABLE [dbo].[Genders] (
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (10) NOT NULL,
    [ShortName] VARCHAR (1)  NOT NULL,
    CONSTRAINT [PK_Genders] PRIMARY KEY CLUSTERED ([Id] ASC)
);

