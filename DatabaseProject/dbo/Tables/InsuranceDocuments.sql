CREATE TABLE [dbo].[InsuranceDocuments] (
    [Id]                      INT          IDENTITY (1, 1) NOT NULL,
    [PersonId]                INT          NOT NULL,
    [InsuranceCompanyId]      INT          NOT NULL,
    [InsuranceDocumentTypeId] INT          NOT NULL,
    [Series]                  VARCHAR (50) CONSTRAINT [DF_InsuranceDocuments_Series] DEFAULT ('') NOT NULL,
    [Number]                  VARCHAR (50) NOT NULL,
    [BeginDate]               DATETIME     NOT NULL,
    [EndDate]                 DATETIME     NOT NULL,
    [DeleteDateTime]          DATETIME     NULL,
    CONSTRAINT [PK_InsuranceDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InsuranceDocuments_InsuranceCompanies] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [dbo].[InsuranceCompanies] ([Id]),
    CONSTRAINT [FK_InsuranceDocuments_InsuranceDocumentTypes] FOREIGN KEY ([InsuranceDocumentTypeId]) REFERENCES [dbo].[InsuranceDocumentTypes] ([Id]),
    CONSTRAINT [FK_InsuranceDocuments_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id])
);









