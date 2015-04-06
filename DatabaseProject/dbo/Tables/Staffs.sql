CREATE TABLE [dbo].[Staffs] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]  INT           NULL,
    [Name]      VARCHAR (500) NOT NULL,
    [ShortName] VARCHAR (100) NOT NULL,
    [IsSenior]  BIT           NOT NULL,
    CONSTRAINT [PK_Staffs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

