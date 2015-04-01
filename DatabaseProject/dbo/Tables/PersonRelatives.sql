CREATE TABLE [dbo].[PersonRelatives] (
    [Id]                     INT IDENTITY (1, 1) NOT NULL,
    [PersonId]               INT NOT NULL,
    [RelativeId]             INT NOT NULL,
    [RelativeRelationshipId] INT NOT NULL,
    [IsRepresentative]       BIT CONSTRAINT [DF_PersonRelatives_IsRepresentative] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PersonRelatives] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonRelatives_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonRelatives_Persons1] FOREIGN KEY ([RelativeId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonRelatives_RelativeRelationships] FOREIGN KEY ([RelativeRelationshipId]) REFERENCES [dbo].[RelativeRelationships] ([Id])
);

