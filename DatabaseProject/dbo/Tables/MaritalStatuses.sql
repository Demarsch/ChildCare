CREATE TABLE [dbo].[MaritalStatuses] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (512) NOT NULL,
    CONSTRAINT [PK_MaritalStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);

