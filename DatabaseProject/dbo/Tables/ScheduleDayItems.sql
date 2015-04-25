CREATE TABLE [dbo].[ScheduleDayItems] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [ScheduleDayId] INT      NOT NULL,
    [StartTime]     TIME (7) NOT NULL,
    [EndTime]       TIME (7) NOT NULL,
    CONSTRAINT [PK_ScheduleDayItem] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ScheduleDayItem_ScheduleDay] FOREIGN KEY ([ScheduleDayId]) REFERENCES [dbo].[ScheduleDays] ([Id])
);

