CREATE TABLE [dbo].[MKBGroups] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [ParentId] INT           NULL,
    [Name]     VARCHAR (500) CONSTRAINT [DF_MKBGroups_Name] DEFAULT ('') NOT NULL,
    [MKBmin]   VARCHAR (5)   CONSTRAINT [DF_MKBGroups_MKBmin] DEFAULT ('') NOT NULL,
    [MKBmax]   VARCHAR (5)   CONSTRAINT [DF_Table_1_MKBmin1] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_MKBGroups] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MKBGroups_MKBGroups] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[MKBGroups] ([Id])
);

