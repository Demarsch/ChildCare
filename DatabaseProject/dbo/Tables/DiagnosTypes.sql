CREATE TABLE [dbo].[DiagnosTypes] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Name]             VARCHAR (500) NOT NULL,
    [Priority]         INT           CONSTRAINT [DF_DiagnosTypes_Priority] DEFAULT ((0)) NOT NULL,
    [IsActual]         BIT           CONSTRAINT [DF_DiagnosTypes_IsActual] DEFAULT ((1)) NOT NULL,
    [AllowCopy]        BIT           CONSTRAINT [DF_DiagnosTypes_AllowCopy] DEFAULT ((0)) NOT NULL,
    [HasComplications] BIT           CONSTRAINT [DF_DiagnosTypes_HasComplications] DEFAULT ((0)) NOT NULL,
    [NeedMKB]          BIT           CONSTRAINT [DF_DiagnosTypes_NeedMKB] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DiagnosTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

