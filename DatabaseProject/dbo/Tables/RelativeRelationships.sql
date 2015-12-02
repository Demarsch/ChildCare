CREATE TABLE [dbo].[RelativeRelationships] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (100) NOT NULL,
    [MustBeMale] BIT           NULL,
    CONSTRAINT [PK_RelativeRelationships] PRIMARY KEY CLUSTERED ([Id] ASC)
);



