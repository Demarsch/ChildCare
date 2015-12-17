CREATE TABLE [dbo].[AnalyseRefferences] (
    [Id]                    INT        IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]          INT        NOT NULL,
    [ParameterRecordTypeId] INT        NOT NULL,
    [IsMale]                BIT        CONSTRAINT [DF_AnalyseRefferences_IsMale] DEFAULT ((1)) NOT NULL,
    [AgeFrom]               INT        CONSTRAINT [DF_AnalyseRefferences_AgeFrom] DEFAULT ((0)) NOT NULL,
    [AgeTo]                 INT        NULL,
    [RefMin]                FLOAT (53) CONSTRAINT [DF_AnalyseRefferences_RefMin] DEFAULT ((0)) NOT NULL,
    [RefMax]                FLOAT (53) CONSTRAINT [DF_AnalyseRefferences_RefMax] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AnalyseRefferences] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AnalyseRefferences_ParameterRecordTypes] FOREIGN KEY ([ParameterRecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]),
    CONSTRAINT [FK_AnalyseRefferences_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id])
);





