CREATE TABLE [dbo].[AnalyseResults] (
    [Id]                    INT           IDENTITY (1, 1) NOT NULL,
    [RecordId]              INT           NOT NULL,
    [ParameterRecordTypeId] INT           NOT NULL,
    [Value]                 VARCHAR (100) CONSTRAINT [DF_AnalyseResults_Value] DEFAULT ('') NOT NULL,
    [UnitId]                INT           NULL,
    [IsNormal]              BIT           CONSTRAINT [DF_AnalyseResults_IsNormal] DEFAULT ((0)) NOT NULL,
    [IsAboveRef]            BIT           CONSTRAINT [DF_AnalyseResults_IsAbove] DEFAULT ((0)) NOT NULL,
    [IsBelowRef]            BIT           CONSTRAINT [DF_AnalyseResults_IsBelow] DEFAULT ((0)) NOT NULL,
    [Details]               VARCHAR (500) CONSTRAINT [DF_AnalyseResults_Details] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_AnalyseResults] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AnalyseResults_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id]),
    CONSTRAINT [FK_AnalyseResults_RecordTypes] FOREIGN KEY ([ParameterRecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_AnalyseResults_Units] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Units] ([Id])
);



