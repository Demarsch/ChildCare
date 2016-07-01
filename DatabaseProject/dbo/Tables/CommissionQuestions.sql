CREATE TABLE [dbo].[CommissionQuestions] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [CommissionTypeId]  INT            NOT NULL,
    [Name]              VARCHAR (8000) NOT NULL,
    [ShortName]         VARCHAR (1000) NOT NULL,
    [PrintedDocumentId] INT            NULL,
    [BeginDateTime]     DATETIME       NOT NULL,
    [EndDateTime]       DATETIME       NOT NULL,
    CONSTRAINT [PK_CommissionQuestions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionQuestions_CommissionTypes] FOREIGN KEY ([CommissionTypeId]) REFERENCES [dbo].[CommissionTypes] ([Id]),
    CONSTRAINT [FK_CommissionQuestions_PrintedDocuments] FOREIGN KEY ([PrintedDocumentId]) REFERENCES [dbo].[PrintedDocuments] ([Id])
);



