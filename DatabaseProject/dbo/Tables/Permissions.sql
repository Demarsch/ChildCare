CREATE TABLE [dbo].[Permissions] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100)  NOT NULL,
    [Description] VARCHAR (8000) NOT NULL,
    [IsGroup]     BIT            NOT NULL,
    [ReadOnly]    BIT            CONSTRAINT [DF_Permissions_ReadOnly] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Permissions_1] PRIMARY KEY CLUSTERED ([Id] ASC)
);



