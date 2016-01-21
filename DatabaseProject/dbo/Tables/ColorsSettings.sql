CREATE TABLE [dbo].[ColorsSettings] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [RGB]         INT           CONSTRAINT [DF_ColorsSettings_RGB] DEFAULT ((0)) NOT NULL,
    [ColorName]   VARCHAR (100) CONSTRAINT [DF_Table_1_Description] DEFAULT ('') NOT NULL,
    [Options]     VARCHAR (500) CONSTRAINT [DF_ColorsSettings_Options] DEFAULT ('') NOT NULL,
    [Description] VARCHAR (500) CONSTRAINT [DF_ColorsSettings_Description] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_ColorsSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);

