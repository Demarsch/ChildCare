CREATE TABLE [dbo].[RecordTypeRoles] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (500)  NOT NULL,
    [ShortName] VARCHAR (100)  CONSTRAINT [DF_RecordTypeRoles_ShortName] DEFAULT ('') NOT NULL,
    [Options]   VARCHAR (5000) NOT NULL,
    CONSTRAINT [PK_RecordTypeRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);



