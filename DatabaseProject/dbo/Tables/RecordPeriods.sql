CREATE TABLE [dbo].[RecordPeriods] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Name]             VARCHAR (50)  NOT NULL,
    [ShortName]        VARCHAR (50)  NOT NULL,
    [ExecutionPlaceId] INT           NOT NULL,
    [Reserved]         VARCHAR (500) NOT NULL,
    [Active]           BIT           CONSTRAINT [DF_RecordPeriods_Active] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_RecordPeriods] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordPeriods_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'''''', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'RecordPeriods', @level2type = N'COLUMN', @level2name = N'Reserved';

