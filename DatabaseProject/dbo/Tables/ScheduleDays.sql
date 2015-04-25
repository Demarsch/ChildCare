CREATE TABLE [dbo].[ScheduleDays] (
    [Id]           INT  IDENTITY (1, 1) NOT NULL,
    [RoomId]       INT  NOT NULL,
    [RecordTypeId] INT  NOT NULL,
    [DayOfWeek]    INT  NOT NULL,
    [BeginDate]    DATE NOT NULL,
    [EndDate]      DATE NOT NULL,
    CONSTRAINT [PK_ScheduleDay] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ScheduleDay_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id]),
    CONSTRAINT [FK_ScheduleDays_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id])
);



