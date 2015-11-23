CREATE TABLE [dbo].[Complications] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]    INT           NULL,
    [Name]        VARCHAR (500) CONSTRAINT [DF_Complications_Name] DEFAULT ('') NOT NULL,
    [DisplayName] VARCHAR (500) CONSTRAINT [DF_Complications_DisplayName] DEFAULT ('') NOT NULL,
    [Options]     VARCHAR (500) CONSTRAINT [DF_Complications_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Complications] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Complications_Complications] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Complications] ([Id])
);

