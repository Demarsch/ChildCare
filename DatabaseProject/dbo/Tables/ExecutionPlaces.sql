CREATE TABLE [dbo].[ExecutionPlaces] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (100) NOT NULL,
    [ShortName] VARCHAR (100) NOT NULL,
    [UseName]   VARCHAR (100) NOT NULL,
    [IsActive]  BIT           CONSTRAINT [DF_ExecutionPlaces_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ExecutionPlaces] PRIMARY KEY CLUSTERED ([Id] ASC)
);



