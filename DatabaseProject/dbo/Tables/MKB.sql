CREATE TABLE [dbo].[MKB] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [GroupId]  INT           NULL,
    [ParentId] INT           NULL,
    [Code]     VARCHAR (10)  CONSTRAINT [DF_MKB_Code] DEFAULT ('') NOT NULL,
    [Name]     VARCHAR (500) CONSTRAINT [DF_MKB_Name] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_MKB] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MKB_MKB] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[MKB] ([Id]),
    CONSTRAINT [FK_MKB_MKBGroups] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[MKBGroups] ([Id])
);











