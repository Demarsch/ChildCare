CREATE TABLE [dbo].[ReportTemplates] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           VARCHAR (100)  NOT NULL,
    [ReportTitle]    VARCHAR (250)  NOT NULL,
    [Description]    VARCHAR (1000) NOT NULL,
    [IsDocXTemplate] BIT            NOT NULL,
    [Template]       TEXT           NULL,
    CONSTRAINT [PK_ReportTemplates] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_ReportTemplates]
    ON [dbo].[ReportTemplates]([Name] ASC);

