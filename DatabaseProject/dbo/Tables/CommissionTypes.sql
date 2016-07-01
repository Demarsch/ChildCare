CREATE TABLE [dbo].[CommissionTypes] (
    [Id]                    INT           IDENTITY (1, 1) NOT NULL,
    [Name]                  VARCHAR (500) NOT NULL,
    [ShortName]             VARCHAR (100) NOT NULL,
    [CommissionTypeGroupId] INT           NOT NULL,
    [BeginDateTime]         DATETIME      NOT NULL,
    [EndDateTime]           DATETIME      NOT NULL,
    [PrintedDocumentId]     INT           NULL,
    CONSTRAINT [PK_CommissionTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommissionTypes_CommissionTypeGroups] FOREIGN KEY ([CommissionTypeGroupId]) REFERENCES [dbo].[CommissionTypeGroups] ([Id]),
    CONSTRAINT [FK_CommissionTypes_PrintedDocuments] FOREIGN KEY ([PrintedDocumentId]) REFERENCES [dbo].[PrintedDocuments] ([Id])
);





