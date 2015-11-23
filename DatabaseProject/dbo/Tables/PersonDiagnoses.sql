CREATE TABLE [dbo].[PersonDiagnoses] (
    [Id]            INT IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT NOT NULL,
    [RecordId]      INT NOT NULL,
    [DiagnosTypeId] INT NOT NULL,
    CONSTRAINT [PK_PersonDiagnoses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonDiagnoses_DiagnosTypes] FOREIGN KEY ([DiagnosTypeId]) REFERENCES [dbo].[DiagnosTypes] ([Id]),
    CONSTRAINT [FK_PersonDiagnoses_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonDiagnoses_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id])
);

