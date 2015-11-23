CREATE TABLE [dbo].[DiagnosLevels] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [Name]           VARCHAR (100) NOT NULL,
    [ShortName]      VARCHAR (10)  NOT NULL,
    [Priority]       INT           NOT NULL,
    [IsActual]       BIT           CONSTRAINT [DF_DiagnosLevels_IsActual] DEFAULT ((1)) NOT NULL,
    [HasMKB]         BIT           CONSTRAINT [DF_DiagnosLevels_HasMKB] DEFAULT ((0)) NOT NULL,
    [IsComplication] BIT           CONSTRAINT [DF_DiagnosLevels_IsComplication] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DiagnosLevels] PRIMARY KEY CLUSTERED ([Id] ASC)
);

