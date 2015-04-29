CREATE TABLE [dbo].[AddressTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_AddressTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

