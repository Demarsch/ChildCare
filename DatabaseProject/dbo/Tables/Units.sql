CREATE TABLE [dbo].[Units] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (100) CONSTRAINT [DF_Units_Name] DEFAULT ('') NOT NULL,
    [ShortName]     VARCHAR (10)  CONSTRAINT [DF_Units_ShortName] DEFAULT ('') NOT NULL,
    [UseForMedWare] BIT           CONSTRAINT [DF_Table_1_UseForAnalyses] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Units] PRIMARY KEY CLUSTERED ([Id] ASC)
);

