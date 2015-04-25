CREATE TABLE [dbo].[ClosedDates] (
    [Id]         INT  IDENTITY (1, 1) NOT NULL,
    [ClosedDate] DATE NOT NULL,
    [MoveToDate] DATE NULL,
    CONSTRAINT [PK_ClosedDates] PRIMARY KEY CLUSTERED ([Id] ASC)
);

