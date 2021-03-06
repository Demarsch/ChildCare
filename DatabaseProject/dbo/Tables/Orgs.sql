﻿CREATE TABLE [dbo].[Orgs] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (500)  NOT NULL,
    [Details]       VARCHAR (1000) CONSTRAINT [DF_Orgs_Details] DEFAULT ('') NOT NULL,
    [IsLpu]         BIT            CONSTRAINT [DF_Orgs_IsLpu] DEFAULT ((0)) NOT NULL,
    [UseInContract] BIT            CONSTRAINT [DF_Orgs_UseInContract] DEFAULT ((0)) NOT NULL,
    [BeginDateTime] DATETIME       CONSTRAINT [DF_Orgs_BeginDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [EndDateTime]   DATETIME       CONSTRAINT [DF_Orgs_EndDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    CONSTRAINT [PK_Orgs] PRIMARY KEY CLUSTERED ([Id] ASC)
);











