CREATE TABLE [dbo].[RecordPeriods] (
    [Id]               INT          IDENTITY (1, 1) NOT NULL,
    [Name]             VARCHAR (50) NOT NULL,
    [ShortName]        VARCHAR (50) NOT NULL,
    [ExecutionPlaceId] INT          NOT NULL,
    [IsDefault]        BIT          CONSTRAINT [DF_RecordPeriods_IsDefault] DEFAULT ((0)) NOT NULL,
    [IsOperation]      BIT          CONSTRAINT [DF_RecordPeriods_IsOperation] DEFAULT ((0)) NOT NULL,
    [BeginDateTime]    DATETIME     CONSTRAINT [DF_RecordPeriods_BeginDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [EndDateTime]      DATETIME     CONSTRAINT [DF_RecordPeriods_EndDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    CONSTRAINT [PK_RecordPeriods] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordPeriods_ExecutionPlaces] FOREIGN KEY ([ExecutionPlaceId]) REFERENCES [dbo].[ExecutionPlaces] ([Id])
);






GO


