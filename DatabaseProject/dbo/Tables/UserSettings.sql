CREATE TABLE [dbo].[UserSettings] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [UserId] INT            NOT NULL,
    [Name]   VARCHAR (500)  NOT NULL,
    [Value]  VARCHAR (8000) NOT NULL,
    CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserSettings_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserSettings]
    ON [dbo].[UserSettings]([UserId] ASC);

