CREATE TABLE [dbo].[ScheduleItem] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [RoomId]       INT      NOT NULL,
    [RecordTypeId] INT      NOT NULL,
    [DayOfWeek]    INT      NOT NULL,
    [BeginDate]    DATE     NOT NULL,
    [EndDate]      DATE     NOT NULL,
    [StartTime]    TIME (7) NOT NULL,
    [EndTime]      TIME (7) NOT NULL,
    CONSTRAINT [PK_ScheduleItem] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ScheduleItem_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_ScheduleItem_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id])
);

