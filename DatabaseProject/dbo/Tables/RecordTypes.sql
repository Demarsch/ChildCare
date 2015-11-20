CREATE TABLE [dbo].[RecordTypes] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [ParentId]          INT            NULL,
    [Code]              VARCHAR (50)   CONSTRAINT [DF_RecordTypes_Code] DEFAULT ('') NOT NULL,
    [Name]              VARCHAR (1000) CONSTRAINT [DF_RecordTypes_Name] DEFAULT ('') NOT NULL,
    [ShortName]         VARCHAR (255)  CONSTRAINT [DF_RecordTypes_ShrotName] DEFAULT ('') NOT NULL,
    [MinDuration]       INT            NOT NULL,
    [Duration]          INT            CONSTRAINT [DF_RecordTypes_Duration] DEFAULT ((0)) NOT NULL,
    [DisplayOrder]      INT            CONSTRAINT [DF_RecordTypes_DisplayOrder] DEFAULT ((0)) NOT NULL,
    [Assignable]        BIT            NULL,
    [RecordTypeGroupId] INT            NOT NULL,
    [MedProfileId]      INT            NOT NULL,
    [IsRecord]          BIT            CONSTRAINT [DF_RecordTypes_IsRecord] DEFAULT ((0)) NOT NULL,
    [Options]           VARCHAR (1000) CONSTRAINT [DF_RecordTypes_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_RecordTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypes_RecordTypes] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[RecordTypes] ([Id])
);











