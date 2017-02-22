CREATE TABLE [dbo].[UserMessages] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [UserMessageTypeId] INT            NOT NULL,
    [SenderUserId]      INT            NOT NULL,
    [SendDateTime]      DATETIME       NOT NULL,
    [RecieverUserId]    INT            NOT NULL,
    [OutDateTime]       DATETIME       NOT NULL,
    [ReadDateTime]      DATETIME       NULL,
    [MessageText]       VARCHAR (8000) NOT NULL,
    [MessageTag]        VARCHAR (8000) NOT NULL,
    [IsHighPriority]    BIT            NOT NULL,
    CONSTRAINT [PK_UserMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserMessages_UserMessageTypes] FOREIGN KEY ([UserMessageTypeId]) REFERENCES [dbo].[UserMessageTypes] ([Id]),
    CONSTRAINT [FK_UserMessages_Users] FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_UserMessages_Users1] FOREIGN KEY ([RecieverUserId]) REFERENCES [dbo].[Users] ([Id])
);



