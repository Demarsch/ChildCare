CREATE TABLE [dbo].[OuterDocumentTypes] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]  INT           NULL,
    [Name]      VARCHAR (500) CONSTRAINT [DF__OuterDocum__Name__4A4E069C] DEFAULT ('') NOT NULL,
    [HasDate]   BIT           NOT NULL,
    [BeginDate] DATETIME      NOT NULL,
    [EndDate]   DATETIME      NOT NULL,
    CONSTRAINT [PK__OuterDoc__3214EC074865BE2A] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OuterDocumentTypes_OuterDocumentTypes] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[OuterDocumentTypes] ([Id])
);

