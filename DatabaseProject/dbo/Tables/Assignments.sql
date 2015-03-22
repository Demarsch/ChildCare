CREATE TABLE [dbo].[Assignments] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [AssignmentTypeId] INT           NOT NULL,
    [PatientId]        INT           NOT NULL,
    [AssignUserId]     INT           NOT NULL,
    [AssignDateTime]   DATETIME      NOT NULL,
    [IsCompleted]      BIT           NOT NULL,
    [Comments]         VARCHAR (300) NULL,
    [CancelDateTime]   DATETIME      NULL,
    [CancelUserId]     INT           NULL,
    CONSTRAINT [PK_Assignment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Assignment_AssignedUsers] FOREIGN KEY ([AssignUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Assignment_AssignmentType] FOREIGN KEY ([AssignmentTypeId]) REFERENCES [dbo].[AssignmentTypes] ([Id]),
    CONSTRAINT [FK_Assignment_CanceledUsers] FOREIGN KEY ([CancelUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Assignment_Persons] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Persons] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Assignment_PatientId]
    ON [dbo].[Assignments]([PatientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Assignment_AssignDateTime]
    ON [dbo].[Assignments]([AssignDateTime] ASC);

