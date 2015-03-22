CREATE TABLE [dbo].[AssignmentTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (200) NOT NULL,
    CONSTRAINT [PK_AssignmentType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

