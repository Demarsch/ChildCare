CREATE TABLE [dbo].[PrintedDocuments] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]         INT           NULL,
    [Name]             VARCHAR (500) NOT NULL,
    [ShortName]        VARCHAR (100) CONSTRAINT [DF_Table_1_ShartName] DEFAULT ('') NOT NULL,
    [ReportTemplateId] INT           NULL,
    [Options]          VARCHAR (500) CONSTRAINT [DF_PrintedDocuments_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_PrintedDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PrintedDocuments_PrintedDocuments] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[PrintedDocuments] ([Id]),
    CONSTRAINT [FK_PrintedDocuments_ReportTemplates] FOREIGN KEY ([ReportTemplateId]) REFERENCES [dbo].[ReportTemplates] ([Id])
);

