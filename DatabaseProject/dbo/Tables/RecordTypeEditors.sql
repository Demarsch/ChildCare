﻿CREATE TABLE [dbo].[RecordTypeEditors] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]  INT           NOT NULL,
    [Editor]        VARCHAR (100) CONSTRAINT [DF_RecordTypeEditors_Editor] DEFAULT ('') NOT NULL,
    [HasDocuments]  BIT           CONSTRAINT [DF_RecordTypeEditors_HasDocuments] DEFAULT ((0)) NOT NULL,
    [HasDICOM]      BIT           CONSTRAINT [DF_RecordTypeEditors_HasDICOM] DEFAULT ((0)) NOT NULL,
    [UseEquipments] BIT           CONSTRAINT [DF_RecordTypeEditors_UseEquipments] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RecordTypeEditors] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypeEditors_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]) ON DELETE CASCADE
);





