CREATE TABLE [dbo].[UserMessages] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [SenderUserId]      INT            NOT NULL,
    [RecieverUserId]    INT            NOT NULL,
    [SendDateTime]      DATETIME       NOT NULL,
    [OutDateTime]       DATETIME       NOT NULL,
    [ReadDateTime]      DATETIME       NOT NULL,
    [MessageText]       VARCHAR (8000) NOT NULL,
    [MessageData]       INT            NOT NULL,
    [UserMessageTypeId] INT            NOT NULL,
    CONSTRAINT [PK_UserMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserMessages_RecieverUsers1] FOREIGN KEY ([RecieverUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_UserMessages_SenderUsers] FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_UserMessages_UserMessageTypes] FOREIGN KEY ([UserMessageTypeId]) REFERENCES [dbo].[UserMessageTypes] ([Id])
);

