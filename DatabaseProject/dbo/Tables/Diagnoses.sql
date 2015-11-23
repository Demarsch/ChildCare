CREATE TABLE [dbo].[Diagnoses] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [PersonDiagnosId] INT            NOT NULL,
    [DiagnosLevelId]  INT            NOT NULL,
    [DiagnosText]     VARCHAR (7000) CONSTRAINT [DF_Diagnoses_DiagnosText] DEFAULT ('') NOT NULL,
    [MKB]             VARCHAR (10)   CONSTRAINT [DF_Diagnoses_MKB] DEFAULT ('') NOT NULL,
    [ComplicationId]  INT            NULL,
    [IsMainDiagnos]   BIT            CONSTRAINT [DF_Diagnoses_IsMainDiagnos] DEFAULT ((0)) NOT NULL,
    [Options]         VARCHAR (500)  CONSTRAINT [DF_Table_1_Optoins] DEFAULT ('') NOT NULL,
    [InDateTime]      DATETIME       NOT NULL,
    [InPersonId]      INT            NOT NULL,
    CONSTRAINT [PK_Diagnoses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Diagnoses_Complications] FOREIGN KEY ([ComplicationId]) REFERENCES [dbo].[Complications] ([Id]),
    CONSTRAINT [FK_Diagnoses_DiagnosLevels] FOREIGN KEY ([DiagnosLevelId]) REFERENCES [dbo].[DiagnosLevels] ([Id]),
    CONSTRAINT [FK_Diagnoses_PersonDiagnoses] FOREIGN KEY ([PersonDiagnosId]) REFERENCES [dbo].[PersonDiagnoses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Diagnoses_Persons] FOREIGN KEY ([InPersonId]) REFERENCES [dbo].[Persons] ([Id])
);

