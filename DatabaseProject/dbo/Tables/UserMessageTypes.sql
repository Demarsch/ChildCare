CREATE TABLE [dbo].[UserMessageTypes] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (500) NOT NULL,
    [ShortName] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_UserMessageTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

