CREATE TABLE [dbo].[ColorsSettings] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [ColorName]   VARCHAR (100) CONSTRAINT [DF_Table_1_Description] DEFAULT ('') NOT NULL,
    [Options]     VARCHAR (500) CONSTRAINT [DF_ColorsSettings_Options] DEFAULT ('') NOT NULL,
    [Description] VARCHAR (500) CONSTRAINT [DF_ColorsSettings_Description] DEFAULT ('') NOT NULL,
    [Hex]         VARCHAR (7)   CONSTRAINT [DF_ColorsSettings_Hex] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_ColorsSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
);





