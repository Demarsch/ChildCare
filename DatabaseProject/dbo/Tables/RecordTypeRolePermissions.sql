﻿CREATE TABLE [dbo].[RecordTypeRolePermissions] (
    [Id]                     INT      IDENTITY (1, 1) NOT NULL,
    [RecordTypeId]           INT      NOT NULL,
    [RecordTypeMemberRoleId] INT      NOT NULL,
    [PermissionId]           INT      NOT NULL,
    [IsSign]                 BIT      CONSTRAINT [DF_RecordTypeRolePermissions_IsSign] DEFAULT ((0)) NOT NULL,
    [IsRequired]             BIT      CONSTRAINT [DF_RecordTypeRolePermissions_IsRequired] DEFAULT ((0)) NOT NULL,
    [BeginDateTime]          DATETIME CONSTRAINT [DF_RecordTypeRolePermissions_BeginDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [EndDateTime]            DATETIME CONSTRAINT [DF_RecordTypeRolePermissions_EndDateTime] DEFAULT (((31)/(12))/(9998)) NOT NULL,
    CONSTRAINT [PK_RecordTypeRolePermissions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordTypeRolePermissions_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]),
    CONSTRAINT [FK_RecordTypeRolePermissions_RecordTypeRolePermissions] FOREIGN KEY ([RecordTypeMemberRoleId]) REFERENCES [dbo].[RecordTypeRoles] ([Id]),
    CONSTRAINT [FK_RecordTypeRolePermissions_RecordTypes] FOREIGN KEY ([RecordTypeId]) REFERENCES [dbo].[RecordTypes] ([Id]) ON DELETE CASCADE
);





